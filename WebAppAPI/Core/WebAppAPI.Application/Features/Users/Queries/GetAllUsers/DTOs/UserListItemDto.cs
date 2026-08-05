namespace WebAppAPI.Application.Features.Users.Queries.GetAllUsers.DTOs
{
    public sealed class UserListItemDto
    {
        public required string Id { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public string? UserName { get; init; }
        public string? Email { get; init; }
        public string? PhoneNumber { get; init; }
        public bool TwoFactorEnabled { get; init; }
        public DateTime? DateCreated { get; init; }
        public DateTime? DateUpdated { get; init; }
    }
}
