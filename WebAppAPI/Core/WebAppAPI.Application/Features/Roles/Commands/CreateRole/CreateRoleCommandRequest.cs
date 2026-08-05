using MediatR;

namespace WebAppAPI.Application.Features.Roles.Commands.CreateRole
{
    public sealed class CreateRoleCommandRequest : IRequest<CreateRoleCommandResponse>
    {
        public required string Name { get; init; }
        public bool IsAdmin { get; init; }
    }
}
