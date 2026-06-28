using MediatR;
using WebAppAPI.Application.Abstractions.Services.Authentications;

namespace WebAppAPI.Application.Features.Auth.Commands.Login
{
    public sealed class LoginCommandHandler(IInternalAuthentication authService) : IRequestHandler<LoginCommandRequest, LoginCommandResponse>
    {
        public async Task<LoginCommandResponse> Handle(LoginCommandRequest request, CancellationToken cancellationToken)
        {
            await authService.LoginAsync(request.UsernameOrEmail, request.Password);

            return new();
        }
    }
}
