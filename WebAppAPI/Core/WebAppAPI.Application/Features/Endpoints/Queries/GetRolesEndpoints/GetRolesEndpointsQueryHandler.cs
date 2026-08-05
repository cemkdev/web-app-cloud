using MediatR;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Features.Endpoints.DTOs;

namespace WebAppAPI.Application.Features.Endpoints.Queries.GetRolesEndpoints
{
    public sealed class GetRolesEndpointsQueryHandler(IEndpointService endpointService) : IRequestHandler<GetRolesEndpointsQueryRequest, GetRolesEndpointsQueryResponse>
    {
        public async Task<GetRolesEndpointsQueryResponse> Handle(GetRolesEndpointsQueryRequest request, CancellationToken cancellationToken)
        {
            List<RolesEndpointsDto> rolesEndpoints = await endpointService.GetRolesEndpointsAsync(cancellationToken);

            return new GetRolesEndpointsQueryResponse
            {
                RolesEndpoints = rolesEndpoints
            };
        }
    }
}
