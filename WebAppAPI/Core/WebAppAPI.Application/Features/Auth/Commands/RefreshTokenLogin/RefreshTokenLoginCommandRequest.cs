using MediatR;

namespace WebAppAPI.Application.Features.Auth.Commands.RefreshTokenLogin
{
    public sealed class RefreshTokenLoginCommandRequest : IRequest<RefreshTokenLoginCommandResponse>
    {
    }
}
