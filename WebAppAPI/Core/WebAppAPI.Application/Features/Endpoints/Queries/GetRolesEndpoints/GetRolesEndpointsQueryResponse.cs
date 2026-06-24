using WebAppAPI.Application.DTOs.Endpoint;

namespace WebAppAPI.Application.Features.Endpoints.Queries.GetRolesEndpoints
{
    public class GetRolesEndpointsQueryResponse
    {
        public List<RolesEndpointsDto> RolesEndpoints { get; set; }
    }
}
