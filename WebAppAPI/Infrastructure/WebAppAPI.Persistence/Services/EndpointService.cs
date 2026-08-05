using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Abstractions.Services.Configurations;
using WebAppAPI.Application.Consts;
using WebAppAPI.Application.DTOs.AuthorizationDefinitions;
using WebAppAPI.Application.Exceptions;
using WebAppAPI.Application.Features.Endpoints.DTOs;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Domain.Entities;
using WebAppAPI.Domain.Entities.Identity;

namespace WebAppAPI.Persistence.Services
{
    public class EndpointService(
        IApplicationService applicationService,
        IMenuReadRepository menuReadRepository,
        IWriteRepository<Menu> menuWriteRepository,
        IEndpointReadRepository endpointReadRepository,
        IWriteRepository<Endpoint> endpointWriteRepository,
        RoleManager<AppRole> roleManager,
        UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork) : IEndpointService
    {
        private readonly IApplicationService _applicationService = applicationService;
        private readonly IMenuReadRepository _menuReadRepository = menuReadRepository;
        private readonly IWriteRepository<Menu> _menuWriteRepository = menuWriteRepository;
        private readonly IEndpointReadRepository _endpointReadRepository = endpointReadRepository;
        private readonly IWriteRepository<Endpoint> _endpointWriteRepository = endpointWriteRepository;
        private readonly RoleManager<AppRole> _roleManager = roleManager;
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<List<RolesEndpointsDto>> GetRolesEndpointsAsync(CancellationToken cancellationToken)
        {
            List<string> roleIds = await _roleManager.Roles
                .AsNoTracking()
                .Select(role => role.Id)
                .ToListAsync(cancellationToken);

            List<Endpoint> endpoints = await _endpointReadRepository.GetAllWithMenuAndRolesAsync(cancellationToken);

            List<RolesEndpointsDto> rolesEndpoints = new();

            foreach (var roleId in roleIds)
            {
                RolesEndpointsDto roleDto = new()
                {
                    RoleId = roleId,
                    RoleEndpoints = endpoints
                        .Select(endpoint => new RoleEndpoint
                        {
                            MenuName = endpoint.Menu.Name,
                            EndpointCode = endpoint.Code,
                            IsAuthorized = endpoint.Roles.Any(role => role.Id == roleId)
                        })
                        .ToList()
                };

                rolesEndpoints.Add(roleDto);
            }

            return rolesEndpoints;
        }

        public async Task<List<RolesEndpointsDto>> GetCurrentUserRoleEndpointsAsync(string username, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(username);

            AppUser? user = await _userManager.FindByNameAsync(username);

            if (user is null)
                throw new NotFoundUserException("Authenticated user could not be found.");

            IList<string> userRoleNames = await _userManager.GetRolesAsync(user);

            if (userRoleNames.Count == 0)
                return [];

            List<string> userRoleIds = await _roleManager.Roles
                .AsNoTracking()
                .Where(role =>
                    role.Name != null &&
                    userRoleNames.Contains(role.Name))
                .Select(role => role.Id)
                .ToListAsync(cancellationToken);

            if (userRoleIds.Count == 0)
                return [];

            List<Endpoint> authorizedEndpoints = await _endpointReadRepository.GetAuthorizedByRoleIdsAsync(userRoleIds, cancellationToken);

            return userRoleIds
                .Select(roleId => new RolesEndpointsDto
                {
                    RoleId = roleId,
                    RoleEndpoints = authorizedEndpoints
                        .Where(endpoint => endpoint.Roles.Any(role => role.Id == roleId))
                        .Select(endpoint => new RoleEndpoint
                        {
                            MenuName = endpoint.Menu.Name,
                            EndpointCode = endpoint.Code,
                            IsAuthorized = true
                        })
                        .ToList()
                })
                .ToList();
        }

        public async Task AssignRoleToEndpointsAsync(List<RolesEndpointsDto> rolesEndpoints, Type type, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(rolesEndpoints);
            ArgumentNullException.ThrowIfNull(type);

            if (rolesEndpoints.Count == 0)
                return;

            // 1. Validate role-level request structure.
            string[] requestedRoleIds = rolesEndpoints
                                        .Select(roleEndpoints => roleEndpoints.RoleId)
                                        .ToArray();

            if (requestedRoleIds.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Every role permission entry must contain a valid role id.", nameof(rolesEndpoints));

            if (requestedRoleIds.Distinct(StringComparer.Ordinal).Count() != requestedRoleIds.Length)
                throw new ArgumentException("The request contains duplicate role entries.", nameof(rolesEndpoints));

            foreach (RolesEndpointsDto roleEndpoints in rolesEndpoints)
            {
                if (roleEndpoints.RoleEndpoints is null)
                    throw new ArgumentException($"Endpoint permissions are missing for role '{roleEndpoints.RoleId}'.", nameof(rolesEndpoints));

                bool hasInvalidEndpoint = roleEndpoints.RoleEndpoints.Any(endpoint =>
                    string.IsNullOrWhiteSpace(endpoint.MenuName) ||
                    string.IsNullOrWhiteSpace(endpoint.EndpointCode));

                if (hasInvalidEndpoint)
                    throw new ArgumentException($"Role '{roleEndpoints.RoleId}' contains an invalid endpoint permission.", nameof(rolesEndpoints));

                bool hasDuplicateEndpoint = roleEndpoints.RoleEndpoints
                    .GroupBy(
                        endpoint => (endpoint.MenuName, endpoint.EndpointCode))
                    .Any(group => group.Count() > 1);

                if (hasDuplicateEndpoint)
                    throw new ArgumentException($"Role '{roleEndpoints.RoleId}' contains duplicate endpoint permissions.", nameof(rolesEndpoints));
            }

            cancellationToken.ThrowIfCancellationRequested();

            // 2. Scan authorization definitions only once.
            List<EndpointMenuDto> endpointMenus = _applicationService.GetAuthorizeDefinitionEndpoints(type);

            Dictionary<(string MenuName, string EndpointCode), EndpointDefinitionDto> endpointDefinitions = endpointMenus
                .SelectMany(endpointMenu => endpointMenu.Endpoints.Select(endpointDefinition => new
                {
                    MenuName = endpointMenu.Name,
                    EndpointDefinition = endpointDefinition
                }))
                .ToDictionary(
                    item => (item.MenuName, item.EndpointDefinition.Code),
                    item => item.EndpointDefinition);

            // Validate all incoming endpoint references before changing tracked entities.
            foreach (RolesEndpointsDto roleEndpoints in rolesEndpoints)
                foreach (RoleEndpoint roleEndpoint in roleEndpoints.RoleEndpoints)
                    if (!endpointDefinitions.ContainsKey((roleEndpoint.MenuName, roleEndpoint.EndpointCode)))
                        throw new ArgumentException($"Authorization definition '{roleEndpoint.MenuName} / {roleEndpoint.EndpointCode}' could not be found.", nameof(rolesEndpoints));

            // 3. Load only the roles included in the request.
            List<AppRole> roles = await _roleManager.Roles
                .Where(role => requestedRoleIds.Contains(role.Id))
                .ToListAsync(cancellationToken);

            if (roles.Count != requestedRoleIds.Length)
            {
                string[] foundRoleIds = roles
                    .Select(role => role.Id)
                    .ToArray();

                string[] missingRoleIds = requestedRoleIds
                    .Except(foundRoleIds, StringComparer.Ordinal)
                    .ToArray();

                throw new ArgumentException($"The following roles could not be found: {string.Join(", ", missingRoleIds)}.", nameof(rolesEndpoints));
            }

            // Prevent removal of bootstrap permissions required to manage role access.
            RolesEndpointsDto? systemAdministratorPermissions = rolesEndpoints
                .FirstOrDefault(roleEndpoints => roleEndpoints.RoleId == SystemBootstrapConstants.SystemAdministratorRoleId);

            RoleEndpoint? protectedPermissionRemoval = systemAdministratorPermissions?.RoleEndpoints.FirstOrDefault(roleEndpoint =>
                    !roleEndpoint.IsAuthorized &&
                    SystemBootstrapConstants.ProtectedEndpointCodes.Contains(roleEndpoint.EndpointCode));

            if (protectedPermissionRemoval is not null)
                throw new InvalidOperationException($"The protected SystemAdministrator permission '{protectedPermissionRemoval.EndpointCode}' cannot be removed.");
            //

            List<Menu> menus = await _menuReadRepository.GetAllMenusAsync(cancellationToken, tracking: true);
            List<Endpoint> endpoints = await _endpointReadRepository.GetAllWithMenuAndRolesAsync(cancellationToken, tracking: true);

            // 4. Build lookup structures.
            Dictionary<string, AppRole> rolesById =
                roles.ToDictionary(role => role.Id, StringComparer.Ordinal);

            Dictionary<string, Menu> menusByName =
                menus.ToDictionary(menu => menu.Name, StringComparer.Ordinal);

            Dictionary<(string MenuName, string EndpointCode), Endpoint> endpointsByKey =
                endpoints.ToDictionary(endpoint => (endpoint.Menu.Name, endpoint.Code));

            // 5. Synchronize endpoint metadata and role relationships in memory.
            foreach (RolesEndpointsDto roleEndpoints in rolesEndpoints)
            {
                cancellationToken.ThrowIfCancellationRequested();

                AppRole role = rolesById[roleEndpoints.RoleId];

                foreach (RoleEndpoint roleEndpoint in roleEndpoints.RoleEndpoints)
                {
                    (string MenuName, string EndpointCode) key = (roleEndpoint.MenuName, roleEndpoint.EndpointCode);
                    EndpointDefinitionDto endpointDefinition = endpointDefinitions[key];

                    if (!menusByName.TryGetValue(roleEndpoint.MenuName, out Menu? menu))
                    {
                        menu = new Menu
                        {
                            Name = roleEndpoint.MenuName
                        };

                        await _menuWriteRepository.AddAsync(menu);
                        menusByName.Add(menu.Name, menu);
                    }

                    if (!endpointsByKey.TryGetValue(key, out Endpoint? endpoint))
                    {
                        endpoint = new Endpoint
                        {
                            ActionType = endpointDefinition.ActionType.ToString(),
                            HttpType = endpointDefinition.HttpType,
                            Definition = endpointDefinition.Definition,
                            Code = endpointDefinition.Code,
                            AdminOnly = endpointDefinition.AdminOnly,
                            Menu = menu
                        };

                        await _endpointWriteRepository.AddAsync(endpoint);
                        endpointsByKey.Add(key, endpoint);
                    }
                    else
                    {
                        endpoint.ActionType = endpointDefinition.ActionType.ToString();
                        endpoint.HttpType = endpointDefinition.HttpType;
                        endpoint.Definition = endpointDefinition.Definition;
                        endpoint.AdminOnly = endpointDefinition.AdminOnly;
                    }

                    bool currentlyAuthorized = endpoint.Roles.Any(assignedRole => assignedRole.Id == role.Id);

                    if (roleEndpoint.IsAuthorized && !currentlyAuthorized)
                    {
                        endpoint.Roles.Add(role);
                    }
                    else if (!roleEndpoint.IsAuthorized && currentlyAuthorized)
                    {
                        endpoint.Roles.Remove(
                            endpoint.Roles.First(
                                assignedRole => assignedRole.Id == role.Id));
                    }
                }
            }

            // 6. Persist the complete metadata + permission operation atomically.
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> HasAccessToMenuAsync(string username, string menuName, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(username);
            ArgumentException.ThrowIfNullOrWhiteSpace(menuName);

            List<RolesEndpointsDto> userRoleEndpoints = await GetCurrentUserRoleEndpointsAsync(username, cancellationToken);

            return userRoleEndpoints
                .SelectMany(role => role.RoleEndpoints)
                .Any(endpoint =>
                    endpoint.IsAuthorized &&
                    string.Equals(endpoint.MenuName, menuName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<string>> GetAccessibleMenuNamesAsync(string username, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(username);

            List<RolesEndpointsDto> userRoleEndpoints = await GetCurrentUserRoleEndpointsAsync(username, cancellationToken);

            return userRoleEndpoints
                .SelectMany(role => role.RoleEndpoints)
                .Where(endpoint => endpoint.IsAuthorized)
                .Select(endpoint => endpoint.MenuName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
