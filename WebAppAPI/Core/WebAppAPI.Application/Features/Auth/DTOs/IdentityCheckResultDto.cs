namespace WebAppAPI.Application.Features.Auth.DTOs
{
    public sealed class IdentityCheckResultDto
    {
        public required string UserId { get; init; }
        public required string Username { get; init; }
        public bool IsAuthenticated { get; init; }
        public DateTime Expiration { get; init; }
        public required string RefreshBeforeTime { get; init; }
        public bool IsAdmin { get; init; }
    }
}
