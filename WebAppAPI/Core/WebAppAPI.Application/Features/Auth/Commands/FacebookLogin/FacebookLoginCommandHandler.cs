using MediatR;
using WebAppAPI.Application.Abstractions.Services.Authentications;

namespace WebAppAPI.Application.Features.Auth.Commands.FacebookLogin
{
    public sealed class FacebookLoginCommandHandler(IExternalAuthentication authService) : IRequestHandler<FacebookLoginCommandRequest, FacebookLoginCommandResponse>
    {
        public async Task<FacebookLoginCommandResponse> Handle(FacebookLoginCommandRequest request, CancellationToken cancellationToken)
        {
            await authService.FacebookLoginAsync(request.AuthToken, cancellationToken);

            return new();
        }
    }
}
