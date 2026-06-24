using Microsoft.AspNetCore.Identity;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Domain.Entities;
using WebAppAPI.Domain.Entities.Identity;

namespace WebAppAPI.Persistence.Services
{
    public class PermissionService(
        IUserService userService,
        IEndpointReadRepository endpointReadRepository,
        RoleManager<AppRole> roleManager) : IPermissionService
    {
        private readonly IUserService _userService = userService;
        private readonly IEndpointReadRepository _endpointReadRepository = endpointReadRepository;
        private readonly RoleManager<AppRole> _roleManager = roleManager;

        public async Task<bool?> GetAdminOnlyByCodeAsync(string code)
        {
            return await _endpointReadRepository.GetAdminOnlyByCodeAsync(code);
        }

        public async Task<bool> HasRolePermissionAsync(string username, string code)
        {
            var userRoles = await _userService.GetRolesByUserIdentifierAsync(username);

            if (!userRoles.Any())
                return false;

            Endpoint? endpoint = await _endpointReadRepository.GetByCodeWithMenuAsync(code);

            if (endpoint == null)
                return false;

            var endpointRoleSet = endpoint.Roles.Select(r => r.Name).ToHashSet();
            foreach (var userRole in userRoles)
            {
                if (endpointRoleSet.Contains(userRole))
                    return true;
            }

            return false;
        }

        public async Task<bool> HasAdminAccessAsync(string username)
        {
            var userRoles = await _userService.GetRolesByUserIdentifierAsync(username);

            foreach (var roleName in userRoles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role?.IsAdmin == true)
                    return true;
            }
            return false;
        }
    }
}
