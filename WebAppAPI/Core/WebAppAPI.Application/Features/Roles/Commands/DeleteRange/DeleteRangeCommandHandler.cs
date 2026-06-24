using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Roles.Commands.DeleteRange
{
    public class DeleteRangeCommandHandler : IRequestHandler<DeleteRangeCommandRequest, DeleteRangeCommandResponse>
    {
        readonly IRoleService _roleService;

        public DeleteRangeCommandHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<DeleteRangeCommandResponse> Handle(DeleteRangeCommandRequest request, CancellationToken cancellationToken)
        {
            foreach (var RemovingRoleId in request.RoleIds)
            {
                await _roleService.DeleteRoleAsync(RemovingRoleId);
            }
            return new();
        }
    }
}
