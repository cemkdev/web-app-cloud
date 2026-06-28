using MediatR;

namespace WebAppAPI.Application.Features.Auth.Commands.GoogleLogin
{
    public sealed class GoogleLoginCommandRequest : IRequest<GoogleLoginCommandResponse>
    {
        public required string IdToken { get; init; }
    }
}
