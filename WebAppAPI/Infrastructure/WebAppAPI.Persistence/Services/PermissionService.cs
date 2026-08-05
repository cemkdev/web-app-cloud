using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Enums;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Domain.Entities.Identity;

namespace WebAppAPI.Persistence.Services
{
    public sealed class PermissionService(
        IUserService userService,
        IEndpointReadRepository endpointReadRepository,
        RoleManager<AppRole> roleManager) : IPermissionService
    {
        private readonly IUserService _userService = userService;
        private readonly IEndpointReadRepository _endpointReadRepository = endpointReadRepository;
        private readonly RoleManager<AppRole> _roleManager = roleManager;

        public Task<bool?> RequiresAdminAccessAsync(string code, CancellationToken cancellationToken)
            => _endpointReadRepository.IsAdminOnlyByCodeAsync(code, cancellationToken);

        public async Task<bool> HasRolePermissionAsync(string username, string code, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(code))
                return false;

            List<string> userRoleNames = await _userService.GetRolesByUserIdentifierAsync(username, UserIdentifierType.Username);

            if (userRoleNames.Count == 0)
                return false;

            return await _endpointReadRepository.HasAnyUserRoleForEndpointAsync(code, userRoleNames, cancellationToken);
        }

        public async Task<bool> HasAdminAccessAsync(string username, CancellationToken cancellationToken)
        {
            List<string> userRoles = await _userService.GetRolesByUserIdentifierAsync(username, UserIdentifierType.Username);

            if (userRoles.Count == 0)
                return false;

            return await _roleManager.Roles
                .AnyAsync(
                    role =>
                        role.IsAdmin &&
                        role.Name != null &&
                        userRoles.Contains(role.Name),
                    cancellationToken);
        }
    }
}
