using MediatR;
using WebAppAPI.Application.Abstractions.Services.Authentications;
using WebAppAPI.Application.Exceptions;

namespace WebAppAPI.Application.Features.Auth.Commands.FacebookLogin
{
    public class FacebookLoginCommandHandler : IRequestHandler<FacebookLoginCommandRequest, FacebookLoginCommandResponse>
    {
        readonly IExternalAuthentication _authService;

        public FacebookLoginCommandHandler(IExternalAuthentication authService)
        {
            _authService = authService;
        }

        public async Task<FacebookLoginCommandResponse> Handle(FacebookLoginCommandRequest request, CancellationToken cancellationToken)
        {
            var token = await _authService.FacebookLoginAsync(request.AuthToken);

            if (token == null)
                throw new NotFoundUserException();
            return new();
        }
    }
}
