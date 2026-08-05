namespace WebAppAPI.Application.Features.Users.Queries.GetAllUsers.DTOs
{
    public sealed class GetAllUsersDto
    {
        public required int TotalUserCount { get; init; }
        public required IReadOnlyList<UserListItemDto> Users { get; init; }
    }
}
