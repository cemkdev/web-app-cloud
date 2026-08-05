using WebAppAPI.Application.Features.Roles.Commands.CreateRole.DTOs;
using WebAppAPI.Application.Features.Roles.Commands.UpdateRole.DTOs;
using WebAppAPI.Application.Features.Roles.DTOs;

namespace WebAppAPI.Application.Abstractions.Services
{
    public interface IRoleService
    {
        Task<List<RoleDto>> GetRolesAsync(CancellationToken cancellationToken);
        Task<RoleDto> GetRoleByIdAsync(string id, CancellationToken cancellationToken);
        Task<bool> CreateRoleAsync(CreateRoleDto model, CancellationToken cancellationToken);
        Task<bool> UpdateRoleAsync(UpdateRoleDto model, CancellationToken cancellationToken);
        Task<bool> DeleteRolesAsync(IReadOnlyCollection<string> roleIds, CancellationToken cancellationToken);
    }
}
