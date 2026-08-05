using MediatR;

namespace WebAppAPI.Application.Features.Roles.Commands.UpdateRole
{
    public sealed class UpdateRoleCommandRequest : IRequest<UpdateRoleCommandResponse>
    {
        public required string Id { get; init; }
        public string? Name { get; init; }
        public bool? IsAdmin { get; init; }
    }
}
