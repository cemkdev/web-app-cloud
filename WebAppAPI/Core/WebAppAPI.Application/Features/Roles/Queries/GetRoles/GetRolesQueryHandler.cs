using MediatR;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Features.Roles.DTOs;

namespace WebAppAPI.Application.Features.Roles.Queries.GetRoles
{
    public sealed class GetRolesQueryHandler(IRoleService roleService) : IRequestHandler<GetRolesQueryRequest, List<GetRolesQueryResponse>>
    {
        public async Task<List<GetRolesQueryResponse>> Handle(GetRolesQueryRequest request, CancellationToken cancellationToken)
        {
            List<RoleDto> roles = await roleService.GetRolesAsync(cancellationToken);

            return roles
                .Select(role => new GetRolesQueryResponse
                {
                    Id = role.Id,
                    Name = role.Name,
                    IsAdmin = role.IsAdmin
                })
                .ToList();
        }
    }
}
