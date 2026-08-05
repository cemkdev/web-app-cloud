using WebAppAPI.Application.Features.Endpoints.DTOs;

namespace WebAppAPI.Application.Abstractions.Services
{
    public interface IEndpointService
    {
        public Task<List<RolesEndpointsDto>> GetRolesEndpointsAsync(CancellationToken cancellationToken);
        Task<List<RolesEndpointsDto>> GetCurrentUserRoleEndpointsAsync(string username, CancellationToken cancellationToken);
        public Task AssignRoleToEndpointsAsync(List<RolesEndpointsDto> rolesEndpoints, Type type, CancellationToken cancellationToken);
        public Task<bool> HasAccessToMenuAsync(string username, string menuName, CancellationToken cancellationToken);
        public Task<List<string>> GetAccessibleMenuNamesAsync(string username, CancellationToken cancellationToken);
    }
}
