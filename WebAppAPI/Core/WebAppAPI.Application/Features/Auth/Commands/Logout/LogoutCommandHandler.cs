using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Auth.Commands.Logout
{
    public sealed class LogoutCommandHandler(IAuthService authService) : IRequestHandler<LogoutCommandRequest, LogoutCommandResponse>
    {
        public async Task<LogoutCommandResponse> Handle(LogoutCommandRequest request, CancellationToken cancellationToken)
        {
            await authService.LogoutAsync();

            return new LogoutCommandResponse
            {
                IsSuccess = true,
                Message = "Logged Out Successfully."
            };
        }
    }
}
