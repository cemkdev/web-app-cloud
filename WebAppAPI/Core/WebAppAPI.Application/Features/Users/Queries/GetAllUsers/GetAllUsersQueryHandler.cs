using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Users.Queries.GetAllUsers
{
    public sealed class GetAllUsersQueryHandler(IUserService userService) : IRequestHandler<GetAllUsersQueryRequest, GetAllUsersQueryResponse>
    {
        public async Task<GetAllUsersQueryResponse> Handle(GetAllUsersQueryRequest request, CancellationToken cancellationToken)
        {
            var result = await userService.GetAllUsersAsync(request.Page, request.Size, cancellationToken);

            return new GetAllUsersQueryResponse
            {
                TotalUserCount = result.TotalUserCount,
                Users = result.Users
            };
        }
    }
}
