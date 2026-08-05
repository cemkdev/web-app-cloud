using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Roles.Commands.DeleteRole
{
    public sealed class DeleteRoleCommandHandler(IRoleService roleService) : IRequestHandler<DeleteRoleCommandRequest, DeleteRoleCommandResponse>
    {
        public async Task<DeleteRoleCommandResponse> Handle(DeleteRoleCommandRequest request, CancellationToken cancellationToken)
        {
            bool succeeded = await roleService.DeleteRolesAsync(
            [request.Id],
            cancellationToken);

            return new DeleteRoleCommandResponse
            {
                Succeeded = succeeded
            };
        }
    }
}
