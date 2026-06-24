using MediatR;
using WebAppAPI.Application.Abstractions.Services.Authentications;
using WebAppAPI.Application.Exceptions;

namespace WebAppAPI.Application.Features.Auth.Commands.GoogleLogin
{
    public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommandRequest, GoogleLoginCommandResponse>
    {
        readonly IExternalAuthentication _authService;

        public GoogleLoginCommandHandler(IExternalAuthentication authService)
        {
            _authService = authService;
        }

        public async Task<GoogleLoginCommandResponse> Handle(GoogleLoginCommandRequest request, CancellationToken cancellationToken)
        {
            var token = await _authService.GoogleLoginAsync(request.IdToken);

            if (token == null)
                throw new NotFoundUserException();
            return new();
        }
    }
}
