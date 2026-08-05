using MediatR;

namespace WebAppAPI.Application.Features.Users.Commands.AssignRoleToUser
{
    public sealed class AssignRoleToUserCommandRequest : IRequest<AssignRoleToUserCommandResponse>
    {
        public required string UserId { get; init; }
        public required IReadOnlyCollection<string> Roles { get; init; }
    }
}
