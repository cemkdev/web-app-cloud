namespace WebAppAPI.Application.Features.Users.Commands.UpdatePassword.DTOs
{
    public sealed class ResetPasswordDto
    {
        public required string UserId { get; init; }
        public required string ResetToken { get; init; }
        public required string NewPassword { get; init; }
    }
}
