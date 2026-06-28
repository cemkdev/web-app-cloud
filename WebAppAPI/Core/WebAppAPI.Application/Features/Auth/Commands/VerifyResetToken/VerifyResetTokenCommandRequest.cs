using MediatR;

namespace WebAppAPI.Application.Features.Auth.Commands.VerifyResetToken
{
    public sealed class VerifyResetTokenCommandRequest : IRequest<VerifyResetTokenCommandResponse>
    {
        public required string ResetToken { get; init; }
        public required string UserId { get; init; }
    }
}
