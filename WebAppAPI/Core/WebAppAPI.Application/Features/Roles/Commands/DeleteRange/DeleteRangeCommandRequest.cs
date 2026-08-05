using MediatR;

namespace WebAppAPI.Application.Features.Roles.Commands.DeleteRange
{
    public sealed class DeleteRangeCommandRequest : IRequest<DeleteRangeCommandResponse>
    {
        public required IReadOnlyCollection<string> RoleIds { get; init; }
    }
}
