using MediatR;
using WebAppAPI.Application.Abstractions.Services.Authentications;

namespace WebAppAPI.Application.Features.Auth.Commands.GoogleLogin
{
    public sealed class GoogleLoginCommandHandler(IExternalAuthentication authService) : IRequestHandler<GoogleLoginCommandRequest, GoogleLoginCommandResponse>
    {
        public async Task<GoogleLoginCommandResponse> Handle(GoogleLoginCommandRequest request, CancellationToken cancellationToken)
        {
            await authService.GoogleLoginAsync(request.IdToken);

            return new();
        }
    }
}
