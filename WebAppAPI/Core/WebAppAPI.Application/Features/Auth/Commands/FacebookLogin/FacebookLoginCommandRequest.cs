using MediatR;

namespace WebAppAPI.Application.Features.Auth.Commands.FacebookLogin
{
    public sealed class FacebookLoginCommandRequest : IRequest<FacebookLoginCommandResponse>
    {
        public required string AuthToken { get; init; }
    }
}
