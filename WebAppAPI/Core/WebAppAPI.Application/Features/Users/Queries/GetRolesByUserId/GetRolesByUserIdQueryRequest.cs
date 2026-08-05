using MediatR;

namespace WebAppAPI.Application.Features.Users.Queries.GetRolesByUserId
{
    public sealed class GetRolesByUserIdQueryRequest : IRequest<List<GetRolesByUserIdQueryResponse>>
    {
        public required string UserId { get; init; }
    }
}
