using MediatR;

namespace WebAppAPI.Application.Features.Auth.Commands.RefreshTokenLogin
{
    public class RefreshTokenLoginCommandRequest : IRequest<RefreshTokenLoginCommandResponse>
    {
        //public string RefreshToken { get; set; }
    }
}
