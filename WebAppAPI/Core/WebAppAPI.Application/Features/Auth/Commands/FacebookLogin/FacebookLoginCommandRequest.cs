using MediatR;

namespace WebAppAPI.Application.Features.Auth.Commands.FacebookLogin
{
    public class FacebookLoginCommandRequest : IRequest<FacebookLoginCommandResponse>
    {
        public string AuthToken { get; set; }
    }
}
