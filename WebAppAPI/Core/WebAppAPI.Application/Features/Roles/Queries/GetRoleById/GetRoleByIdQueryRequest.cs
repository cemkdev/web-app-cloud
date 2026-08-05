using MediatR;

namespace WebAppAPI.Application.Features.Roles.Queries.GetRoleById
{
    public sealed class GetRoleByIdQueryRequest : IRequest<GetRoleByIdQueryResponse>
    {
        public required string Id { get; init; }
    }
}
