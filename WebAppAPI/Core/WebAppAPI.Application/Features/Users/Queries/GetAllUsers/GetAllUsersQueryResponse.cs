using WebAppAPI.Application.Features.Users.Queries.GetAllUsers.DTOs;

namespace WebAppAPI.Application.Features.Users.Queries.GetAllUsers
{
    public sealed class GetAllUsersQueryResponse
    {
        public required int TotalUserCount { get; init; }
        public required IReadOnlyList<UserListItemDto> Users { get; init; }
    }
}
