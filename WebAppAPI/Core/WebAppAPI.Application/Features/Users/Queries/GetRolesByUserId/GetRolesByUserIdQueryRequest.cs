using MediatR;

namespace WebAppAPI.Application.Features.Users.Queries.GetRolesByUserId
{
    public class GetRolesByUserIdQueryRequest : IRequest<List<GetRolesByUserIdQueryResponse>>
    {
        public string UserId { get; set; }
    }
}
