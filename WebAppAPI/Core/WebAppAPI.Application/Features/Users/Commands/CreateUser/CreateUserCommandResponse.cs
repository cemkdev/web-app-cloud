namespace WebAppAPI.Application.Features.Users.Commands.CreateUser
{
    public sealed class CreateUserCommandResponse
    {
        public required bool Succeeded { get; init; }
        public required string Message { get; init; }
    }
}
