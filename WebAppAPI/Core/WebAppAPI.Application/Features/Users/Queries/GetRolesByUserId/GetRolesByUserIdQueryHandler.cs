using MediatR;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Enums;
using WebAppAPI.Application.Features.Roles.DTOs;

namespace WebAppAPI.Application.Features.Users.Queries.GetRolesByUserId
{
    public sealed class GetRolesByUserIdQueryHandler(
        IUserService userService,
        IRoleService roleService) : IRequestHandler<GetRolesByUserIdQueryRequest, List<GetRolesByUserIdQueryResponse>>
    {
        public async Task<List<GetRolesByUserIdQueryResponse>> Handle(GetRolesByUserIdQueryRequest request, CancellationToken cancellationToken)
        {
            List<string> userRoles = await userService.GetRolesByUserIdentifierAsync(request.UserId, UserIdentifierType.Id);
            List<RoleDto> allRoles = await roleService.GetRolesAsync(cancellationToken);

            return allRoles
                .Select(role => new GetRolesByUserIdQueryResponse
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    IsAdmin = role.IsAdmin,
                    IsAssigned = userRoles.Contains(role.Name)
                })
                .ToList();
        }
    }
}
