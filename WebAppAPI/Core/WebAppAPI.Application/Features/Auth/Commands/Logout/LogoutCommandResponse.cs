namespace WebAppAPI.Application.Features.Auth.Commands.Logout
{
    public sealed class LogoutCommandResponse
    {
        public bool IsSuccess { get; init; }
        public string? Message { get; init; }
    }
}
