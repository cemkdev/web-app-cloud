using MediatR;

namespace WebAppAPI.Application.Features.Users.Queries.GetAllUsers
{
    public sealed class GetAllUsersQueryRequest : IRequest<GetAllUsersQueryResponse>
    {
        public int Page { get; set; } = 0;
        public int Size { get; set; } = 5;
    }
}
