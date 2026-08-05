namespace WebAppAPI.Application.Features.Users.Commands.CreateUser.DTOs
{
    public sealed class CreateUserResultDto
    {
        public bool Succeeded { get; init; }
        public required string Message { get; init; }
    }
}
