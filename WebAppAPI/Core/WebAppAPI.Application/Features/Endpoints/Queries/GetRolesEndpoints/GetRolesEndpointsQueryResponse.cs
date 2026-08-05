using WebAppAPI.Application.Features.Endpoints.DTOs;

namespace WebAppAPI.Application.Features.Endpoints.Queries.GetRolesEndpoints
{
    public sealed class GetRolesEndpointsQueryResponse
    {
        public required IReadOnlyList<RolesEndpointsDto> RolesEndpoints { get; init; }
    }
}
