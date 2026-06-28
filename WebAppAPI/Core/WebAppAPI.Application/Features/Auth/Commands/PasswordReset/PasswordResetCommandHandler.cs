using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Auth.Commands.PasswordReset
{
    public sealed class PasswordResetCommandHandler(IAuthService authService) : IRequestHandler<PasswordResetCommandRequest, PasswordResetCommandResponse>
    {
        public async Task<PasswordResetCommandResponse> Handle(PasswordResetCommandRequest request, CancellationToken cancellationToken)
        {
            await authService.PasswordResetAsync(request.Email);

            return new();
        }
    }
}
