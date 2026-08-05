using MediatR;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Features.Roles.Commands.CreateRole.DTOs;

namespace WebAppAPI.Application.Features.Roles.Commands.CreateRole
{
    public sealed class CreateRoleCommandHandler(IRoleService roleService) : IRequestHandler<CreateRoleCommandRequest, CreateRoleCommandResponse>
    {
        public async Task<CreateRoleCommandResponse> Handle(CreateRoleCommandRequest request, CancellationToken cancellationToken)
        {
            bool succeeded = await roleService.CreateRoleAsync(
                new CreateRoleDto
                {
                    Name = request.Name,
                    IsAdmin = request.IsAdmin
                },
                cancellationToken);

            return new CreateRoleCommandResponse
            {
                Succeeded = succeeded
            };
        }
    }
}
