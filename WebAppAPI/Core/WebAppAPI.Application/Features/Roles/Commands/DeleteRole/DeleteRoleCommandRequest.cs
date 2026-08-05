using MediatR;

namespace WebAppAPI.Application.Features.Roles.Commands.DeleteRole
{
    public sealed class DeleteRoleCommandRequest : IRequest<DeleteRoleCommandResponse>
    {
        public required string Id { get; init; }
    }
}
