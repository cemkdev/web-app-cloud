using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.DTOs.Role;
using WebAppAPI.Domain.Entities.Identity;

namespace WebAppAPI.Persistence.Services
{
    public class RoleService(RoleManager<AppRole> roleManager) : IRoleService
    {
        private readonly RoleManager<AppRole> _roleManager = roleManager;

        public async Task<List<RoleGetDto>> GetRolesAsync()
        {
            var roles = await _roleManager.Roles.OrderBy(r => r.DateCreated).ToListAsync();

            return roles.Select(r => new RoleGetDto()
            {
                Id = r.Id,
                Name = r.Name,
                IsAdmin = r.IsAdmin
            }).ToList();
        }

        public async Task<(string id, string name, bool isAdmin)> GetRoleByIdAsync(string id)
        {
            AppRole role = await _roleManager.FindByIdAsync(id);
            string name = await _roleManager.GetRoleNameAsync(role);
            return (id, role.Name, role.IsAdmin);
        }

        public async Task<bool> CreateRoleAsync(string name, bool isAdmin)
        {
            IdentityResult result = await _roleManager.CreateAsync(new() { Id = Guid.NewGuid().ToString(), Name = name, IsAdmin = isAdmin });
            return result.Succeeded;
        }

        public async Task<bool> UpdateRoleAsync(string id, string name, bool isAdmin)
        {
            AppRole role = await _roleManager.FindByIdAsync(id);
            role.Name = name;
            role.IsAdmin = isAdmin;
            IdentityResult result = await _roleManager.UpdateAsync(role);
            return result.Succeeded;
        }

        public async Task<bool> DeleteRoleAsync(string id)
        {
            AppRole role = await _roleManager.FindByIdAsync(id);
            IdentityResult result = await _roleManager.DeleteAsync(role);
            return result.Succeeded;
        }
    }
}
