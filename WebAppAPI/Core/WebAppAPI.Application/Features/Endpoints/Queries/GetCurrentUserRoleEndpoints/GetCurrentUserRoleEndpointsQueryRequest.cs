using MediatR;

namespace WebAppAPI.Application.Features.Endpoints.Queries.GetCurrentUserRoleEndpoints
{
    public sealed class GetCurrentUserRoleEndpointsQueryRequest : IRequest<GetCurrentUserRoleEndpointsQueryResponse>
    {
        public required string Username { get; init; }
    }
}
