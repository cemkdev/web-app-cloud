using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Auth.Commands.VerifyResetToken
{
    public sealed class VerifyResetTokenCommandHandler(IAuthService authService) : IRequestHandler<VerifyResetTokenCommandRequest, VerifyResetTokenCommandResponse>
    {
        public async Task<VerifyResetTokenCommandResponse> Handle(VerifyResetTokenCommandRequest request, CancellationToken cancellationToken)
        {
            bool state = await authService.VerifyResetTokenAsync(request.ResetToken, request.UserId);

            return new VerifyResetTokenCommandResponse
            {
                State = state,
            };
        }
    }
}
