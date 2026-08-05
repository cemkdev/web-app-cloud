using MediatR;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Features.Roles.Commands.UpdateRole.DTOs;

namespace WebAppAPI.Application.Features.Roles.Commands.UpdateRole
{
    public sealed class UpdateRoleCommandHandler(IRoleService roleService) : IRequestHandler<UpdateRoleCommandRequest, UpdateRoleCommandResponse>
    {
        public async Task<UpdateRoleCommandResponse> Handle(UpdateRoleCommandRequest request, CancellationToken cancellationToken)
        {
            bool succeeded = await roleService.UpdateRoleAsync(
                new UpdateRoleDto
                {
                    Id = request.Id,
                    Name = request.Name,
                    IsAdmin = request.IsAdmin
                },
                cancellationToken);

            return new UpdateRoleCommandResponse
            {
                Succeeded = succeeded
            };
        }
    }
}
