using MediatR;
using WebAppAPI.Application.Abstractions.Services.Authentications;
using WebAppAPI.Application.Exceptions;

namespace WebAppAPI.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommandRequest, LoginCommandResponse>
    {
        readonly IInternalAuthentication _authService;

        public LoginCommandHandler(IInternalAuthentication authService)
        {
            _authService = authService;
        }

        public async Task<LoginCommandResponse> Handle(LoginCommandRequest request, CancellationToken cancellationToken)
        {
            var token = await _authService.LoginAsync(request.UsernameOrEmail, request.Password);

            if (token == null)
                throw new NotFoundUserException();
            return new();
        }
    }
}
