using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Abstractions.Services.Configurations;
using WebAppAPI.Application.DTOs.Endpoint;
using WebAppAPI.Application.Exceptions;
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

        public async Task<List<RolesEndpointsDto>> GetRolesEndpointsAsync()
        {
            var roles = await _roleManager.Roles
                                .Include(r => r.Endpoints)
                                .ToListAsync();

            List<Endpoint> endpoints = await _endpointReadRepository.GetAllWithMenuAndRolesAsync();

            var rolesEndpoints = new List<RolesEndpointsDto>();

            foreach (var role in roles)
            {
                var roleDto = new RolesEndpointsDto
                {
                    RoleId = role.Id,
                    RoleEndpoints = new List<RoleEndpoint>()
                };

                foreach (var endpoint in endpoints)
                {
                    roleDto.RoleEndpoints.Add(new RoleEndpoint
                    {
                        MenuName = endpoint.Menu.Name,
                        EndpointCode = endpoint.Code,
                        IsAuthorized = endpoint.Roles.Any(r => r.Id == role.Id)
                    });
                }

                rolesEndpoints.Add(roleDto);
            }

            return rolesEndpoints;
        }

        public async Task AssignRoleToEndpointsAsync(List<RolesEndpointsDto> rolesEndpoints, Type type)
        {
            List<Menu> menus = await _menuReadRepository.GetAllMenusAsync();
            List<Endpoint> endpoints = await _endpointReadRepository.GetAllWithMenuAndRolesAsync();
            var roles = await _roleManager.Roles
                                    .Include(r => r.Endpoints).ToListAsync();

            foreach (var roleEndpoints in rolesEndpoints)
            {
                AppRole role = roles.First(r => r.Id == roleEndpoints.RoleId);

                foreach (var roleEndpoint in roleEndpoints.RoleEndpoints)
                {
                    Menu? menu = menus.FirstOrDefault(m => m.Name == roleEndpoint.MenuName);

                    // Create the menu metadata if it does not exist yet.
                    if (menu == null)
                    {
                        menu = new()
                        {
                            Name = roleEndpoint.MenuName
                        };
                        await _menuWriteRepository.AddAsync(menu);
                        await _unitOfWork.SaveChangesAsync();
                        menus.Add(menu);
                    }

                    Endpoint? endpoint = endpoints.FirstOrDefault(e => e.Code == roleEndpoint.EndpointCode && e.Menu.Name == roleEndpoint.MenuName);
                    var action = _applicationService.GetAuthorizeDefinitionEndpoints(type)
                                                .FirstOrDefault(menu => menu.Name == roleEndpoint.MenuName)?
                                                .Actions.FirstOrDefault(a => a.Code == roleEndpoint.EndpointCode);

                    // Create the endpoint metadata if it does not exist yet.
                    if (endpoint == null)
                    {
                        endpoint = new()
                        {
                            ActionType = action.ActionType.ToString(),
                            HttpType = action.HttpType,
                            Definition = action.Definition,
                            Code = action.Code,
                            AdminOnly = action.AdminOnly,
                            Menu = menu
                        };

                        await _endpointWriteRepository.AddAsync(endpoint);
                        await _unitOfWork.SaveChangesAsync();
                        endpoints.Add(endpoint);
                    }
                    else // Keep existing endpoint metadata in sync with the current authorization definition.
                    {
                        var updated = false;

                        if (endpoint.ActionType != action.ActionType.ToString())
                        {
                            endpoint.ActionType = action.ActionType.ToString();
                            updated = true;
                        }

                        if (endpoint.HttpType != action.HttpType)
                        {
                            endpoint.HttpType = action.HttpType;
                            updated = true;
                        }

                        if (endpoint.Definition != action.Definition)
                        {
                            endpoint.Definition = action.Definition;
                            updated = true;
                        }

                        if (endpoint.AdminOnly != action.AdminOnly)
                        {
                            endpoint.AdminOnly = action.AdminOnly;
                            updated = true;
                        }

                        if (updated)
                        {
                            _endpointWriteRepository.Update(endpoint);
                        }
                    }
                    // Sync the endpoint-role relationship.
                    bool hasPermission = endpoint.Roles.Any(r => r.Id == role.Id);

                    if (roleEndpoint.IsAuthorized && !hasPermission)
                    {
                        endpoint.Roles.Add(role);
                    }
                    else if (!roleEndpoint.IsAuthorized && hasPermission)
                    {
                        endpoint.Roles.Remove(role);
                    }
                }
            }
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> HasAccessToMenuAsync(string username, string menuName)
        {
            var filteredEndpointsByUserRoles = await FilteredEndpointsByUserRolesAsync(username);

            foreach (var role in filteredEndpointsByUserRoles)
            {
                foreach (var endpoint in role.RoleEndpoints)
                {
                    if (endpoint.MenuName == menuName && endpoint.IsAuthorized)
                        return true;
                }
            }
            return false;
        }

        public async Task<List<string>> GetAccessibleMenuNamesAsync(string username)
        {
            var filteredEndpointsByUserRoles = await FilteredEndpointsByUserRolesAsync(username);

            var accessibleMenus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var role in filteredEndpointsByUserRoles)
            {
                foreach (var endpoint in role.RoleEndpoints)
                {
                    if (endpoint.IsAuthorized)
                        accessibleMenus.Add(endpoint.MenuName);
                }
            }
            return accessibleMenus.ToList();
        }

        #region Helpers
        public async Task<List<RolesEndpointsDto>> FilteredEndpointsByUserRolesAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new NotFoundUserException();

            var userRoleNames = await _userManager.GetRolesAsync(user);

            var allRolesEndpoints = await GetRolesEndpointsAsync();

            var userRolesEndpoints = allRolesEndpoints
                                        .Where(r => userRoleNames.Any(roleName => _roleManager.Roles.Any(dbRole => dbRole.Id == r.RoleId && dbRole.Name == roleName)))
                                        .ToList();
            return userRolesEndpoints;
        }
        #endregion
    }
}
