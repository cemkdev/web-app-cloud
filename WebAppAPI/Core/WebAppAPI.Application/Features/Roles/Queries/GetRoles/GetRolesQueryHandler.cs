using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Roles.Queries.GetRoles
{
    public class GetRolesQueryHandler : IRequestHandler<GetRolesQueryRequest, List<GetRolesQueryResponse>>
    {
        readonly IRoleService _roleService;

        public GetRolesQueryHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<List<GetRolesQueryResponse>> Handle(GetRolesQueryRequest request, CancellationToken cancellationToken)
        {
            var roles = await _roleService.GetRolesAsync();
            var result = roles.Select(role => new GetRolesQueryResponse
            {
                Id = role.Id,
                Name = role.Name,
                IsAdmin = role.IsAdmin
            }).ToList();

            return result;
        }
    }
}
