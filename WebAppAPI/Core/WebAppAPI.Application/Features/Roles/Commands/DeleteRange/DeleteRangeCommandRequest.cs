using MediatR;

namespace WebAppAPI.Application.Features.Roles.Commands.DeleteRange
{
    public class DeleteRangeCommandRequest : IRequest<DeleteRangeCommandResponse>
    {
        public List<string> RoleIds { get; set; }
    }
}
