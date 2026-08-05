using WebAppAPI.Application.Features.Endpoints.DTOs;

namespace WebAppAPI.Application.Features.Endpoints.Queries.GetCurrentUserRoleEndpoints
{
    public sealed class GetCurrentUserRoleEndpointsQueryResponse
    {
        public required List<RolesEndpointsDto> RolesEndpoints { get; init; }
    }
}
