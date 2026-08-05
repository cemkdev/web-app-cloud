using MediatR;

namespace WebAppAPI.Application.Features.Users.Commands.CreateUser
{
    public sealed class CreateUserCommandRequest : IRequest<CreateUserCommandResponse>
    {
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string Username { get; init; }
        public required string Email { get; init; }
        public required string PhoneNumber { get; init; }
        public required string Password { get; init; }
        public required string ConfirmPassword { get; init; }
    }
}