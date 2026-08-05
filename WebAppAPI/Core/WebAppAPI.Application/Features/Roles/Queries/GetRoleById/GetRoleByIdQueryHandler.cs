using MediatR;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Features.Roles.DTOs;

namespace WebAppAPI.Application.Features.Roles.Queries.GetRoleById
{
    public class GetRoleByIdQueryHandler(IRoleService roleService) : IRequestHandler<GetRoleByIdQueryRequest, GetRoleByIdQueryResponse>
    {
        public async Task<GetRoleByIdQueryResponse> Handle(GetRoleByIdQueryRequest request, CancellationToken cancellationToken)
        {
            RoleDto role = await roleService.GetRoleByIdAsync(request.Id, cancellationToken);

            return new GetRoleByIdQueryResponse
            {
                Id = role.Id,
                Name = role.Name,
                IsAdmin = role.IsAdmin
            };
        }
    }
}
