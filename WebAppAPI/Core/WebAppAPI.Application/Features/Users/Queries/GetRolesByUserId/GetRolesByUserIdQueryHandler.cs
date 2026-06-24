using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Users.Queries.GetRolesByUserId
{
    public class GetRolesByUserIdQueryHandler : IRequestHandler<GetRolesByUserIdQueryRequest, List<GetRolesByUserIdQueryResponse>>
    {
        readonly IUserService _userService;
        readonly IRoleService _roleService;

        public GetRolesByUserIdQueryHandler(IUserService userService, IRoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        public async Task<List<GetRolesByUserIdQueryResponse>> Handle(GetRolesByUserIdQueryRequest request, CancellationToken cancellationToken)
        {
            var userRoles = await _userService.GetRolesByUserIdentifierAsync(request.UserId);
            var allRoles = await _roleService.GetRolesAsync();

            var result = allRoles.Select(roles => new GetRolesByUserIdQueryResponse
            {
                RoleId = roles.Id,
                RoleName = roles.Name,
                IsAdmin = roles.IsAdmin,
                IsAssigned = userRoles.Contains(roles.Name)
            }).ToList();

            return result;
        }
    }
}
