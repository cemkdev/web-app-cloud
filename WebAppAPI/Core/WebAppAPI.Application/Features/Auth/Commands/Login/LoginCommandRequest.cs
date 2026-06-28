using MediatR;

namespace WebAppAPI.Application.Features.Auth.Commands.Login
{
    public sealed class LoginCommandRequest : IRequest<LoginCommandResponse>
    {
        public required string UsernameOrEmail { get; init; }
        public required string Password { get; init; }
    }
}
