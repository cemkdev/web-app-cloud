using MediatR;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Features.Users.Commands.AssignRoleToUser.DTOs;

namespace WebAppAPI.Application.Features.Users.Commands.AssignRoleToUser
{
    public sealed class AssignRoleToUserCommandHandler(IUserService userService) : IRequestHandler<AssignRoleToUserCommandRequest, AssignRoleToUserCommandResponse>
    {
        public async Task<AssignRoleToUserCommandResponse> Handle(AssignRoleToUserCommandRequest request, CancellationToken cancellationToken)
        {
            await userService.AssignRoleToUserAsync(new AssignRolesToUserDto
            {
                UserId = request.UserId,
                Roles = request.Roles
            });

            return new AssignRoleToUserCommandResponse();
        }
    }
}
