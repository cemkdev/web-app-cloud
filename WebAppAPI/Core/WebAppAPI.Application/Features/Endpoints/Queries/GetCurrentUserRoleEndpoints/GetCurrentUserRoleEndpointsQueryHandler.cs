using MediatR;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Features.Endpoints.DTOs;

namespace WebAppAPI.Application.Features.Endpoints.Queries.GetCurrentUserRoleEndpoints
{
    public sealed class GetCurrentUserRoleEndpointsQueryHandler(IEndpointService endpointService) :
        IRequestHandler<GetCurrentUserRoleEndpointsQueryRequest, GetCurrentUserRoleEndpointsQueryResponse>
    {
        public async Task<GetCurrentUserRoleEndpointsQueryResponse> Handle(GetCurrentUserRoleEndpointsQueryRequest request, CancellationToken cancellationToken)
        {
            List<RolesEndpointsDto> rolesEndpoints = await endpointService.GetCurrentUserRoleEndpointsAsync(request.Username, cancellationToken);

            return new GetCurrentUserRoleEndpointsQueryResponse
            {
                RolesEndpoints = rolesEndpoints
            };
        }
    }
}
