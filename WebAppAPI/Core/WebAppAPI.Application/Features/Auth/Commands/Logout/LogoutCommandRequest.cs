using MediatR;

namespace WebAppAPI.Application.Features.Auth.Commands.Logout
{
    public sealed class LogoutCommandRequest : IRequest<LogoutCommandResponse>
    {
    }
}
