using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Roles.Commands.DeleteRange
{
    public sealed class DeleteRangeCommandHandler(IRoleService roleService) : IRequestHandler<DeleteRangeCommandRequest, DeleteRangeCommandResponse>
    {
        public async Task<DeleteRangeCommandResponse> Handle(DeleteRangeCommandRequest request, CancellationToken cancellationToken)
        {
            bool succeeded = await roleService.DeleteRolesAsync(
            request.RoleIds,
            cancellationToken);

            return new DeleteRangeCommandResponse
            {
                Succeeded = succeeded
            };
        }
    }
}
