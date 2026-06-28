namespace WebAppAPI.Application.Features.Auth.DTOs
{
    public sealed class AccessTokenResultDto
    {
        public required string AccessToken { get; init; }
        public DateTime Expiration { get; init; }
    }
}
