using MediatR;

namespace WebAppAPI.Application.Features.Auth.Commands.PasswordReset
{
    public sealed class PasswordResetCommandRequest : IRequest<PasswordResetCommandResponse>
    {
        public required string Email { get; init; }
    }
}
