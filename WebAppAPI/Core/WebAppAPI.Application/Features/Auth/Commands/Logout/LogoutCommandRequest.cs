using MediatR;

namespace WebAppAPI.Application.Features.Auth.Commands.Logout
{
    public class LogoutCommandRequest : IRequest<LogoutCommandResponse>
    {
    }
}
