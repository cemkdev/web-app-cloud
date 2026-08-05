using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Consts;
using WebAppAPI.Application.Features.Roles.Commands.CreateRole.DTOs;
using WebAppAPI.Application.Features.Roles.Commands.UpdateRole.DTOs;
using WebAppAPI.Application.Features.Roles.DTOs;
using WebAppAPI.Domain.Entities.Identity;

namespace WebAppAPI.Persistence.Services
{
    public class RoleService(RoleManager<AppRole> roleManager) : IRoleService
    {
        private readonly RoleManager<AppRole> _roleManager = roleManager;

        public Task<List<RoleDto>> GetRolesAsync(CancellationToken cancellationToken)
            => _roleManager.Roles
                .AsNoTracking()
                .OrderBy(role => role.DateCreated)
                .Select(role => new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name!,
                    IsAdmin = role.IsAdmin
                })
                .ToListAsync(cancellationToken);

        public async Task<RoleDto> GetRoleByIdAsync(string id, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);

            RoleDto? role = await _roleManager.Roles
                .AsNoTracking()
                .Where(role => role.Id == id)
                .Select(role => new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name!,
                    IsAdmin = role.IsAdmin
                })
                .SingleOrDefaultAsync(cancellationToken);

            return role ?? throw new KeyNotFoundException($"Role with ID '{id}' was not found.");
        }

        public async Task<bool> CreateRoleAsync(CreateRoleDto model, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentException.ThrowIfNullOrWhiteSpace(model.Name);

            cancellationToken.ThrowIfCancellationRequested();

            IdentityResult result = await _roleManager.CreateAsync(new AppRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.Name.Trim(),
                IsAdmin = model.IsAdmin
            });

            return result.Succeeded;
        }

        public async Task<bool> UpdateRoleAsync(UpdateRoleDto model, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentException.ThrowIfNullOrWhiteSpace(model.Id);

            if (model.Name is not null && string.IsNullOrWhiteSpace(model.Name))
                throw new ArgumentException("Role name cannot be empty.", nameof(model));

            cancellationToken.ThrowIfCancellationRequested();

            AppRole? role = await _roleManager.FindByIdAsync(model.Id);

            if (role is null)
                throw new KeyNotFoundException($"Role with ID '{model.Id}' was not found.");

            // Keep the bootstrap administrator role name and admin status unchanged.
            if (role.Id == SystemBootstrapConstants.SystemAdministratorRoleId)
            {
                if (model.Name is not null && !string.Equals(model.Name.Trim(), SystemBootstrapConstants.SystemAdministratorRoleName, StringComparison.Ordinal))
                    throw new InvalidOperationException("The SystemAdministrator role name cannot be changed.");

                if (model.IsAdmin == false)
                    throw new InvalidOperationException("Admin access cannot be removed from the SystemAdministrator role.");
            }
            //

            bool hasChanges = false;

            if (model.Name is not null)
            {
                string name = model.Name.Trim();

                if (!string.Equals(role.Name, name, StringComparison.Ordinal))
                {
                    role.Name = name;
                    hasChanges = true;
                }
            }

            if (model.IsAdmin.HasValue &&
                role.IsAdmin != model.IsAdmin.Value)
            {
                role.IsAdmin = model.IsAdmin.Value;
                hasChanges = true;
            }

            if (!hasChanges)
                return true;

            cancellationToken.ThrowIfCancellationRequested();

            IdentityResult result = await _roleManager.UpdateAsync(role);

            return result.Succeeded;
        }

        // TODO Section 4: Review bulk role deletion for partial-failure/cancellation handling.
        // Decide whether transaction support or explicit partial-result reporting is worth adding.
        public async Task<bool> DeleteRolesAsync(IReadOnlyCollection<string> roleIds, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(roleIds);

            string[] distinctRoleIds = roleIds
                .Where(roleId => !string.IsNullOrWhiteSpace(roleId))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (distinctRoleIds.Length == 0)
                throw new ArgumentException("At least one valid role ID must be provided.", nameof(roleIds));

            // Prevent deletion of the bootstrap administrator role.
            if (distinctRoleIds.Contains(SystemBootstrapConstants.SystemAdministratorRoleId, StringComparer.Ordinal))
                throw new InvalidOperationException("The SystemAdministrator role cannot be deleted.");
            //

            foreach (string roleId in distinctRoleIds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                AppRole? role = await _roleManager.FindByIdAsync(roleId) ?? throw new KeyNotFoundException($"Role with ID '{roleId}' was not found.");

                IdentityResult result = await _roleManager.DeleteAsync(role);

                if (!result.Succeeded)
                    return false;
            }

            return true;
        }
    }
}
