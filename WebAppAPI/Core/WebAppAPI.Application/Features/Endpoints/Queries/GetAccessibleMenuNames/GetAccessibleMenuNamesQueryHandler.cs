using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Endpoints.Queries.GetAccessibleMenuNames
{
    public sealed class GetAccessibleMenuNamesQueryHandler(
        IPermissionService permissionService,
        IEndpointService endpointService) : IRequestHandler<GetAccessibleMenuNamesQueryRequest, List<string>>
    {
        public async Task<List<string>> Handle(GetAccessibleMenuNamesQueryRequest request, CancellationToken cancellationToken)
        {
            bool hasAdminAccess = await permissionService.HasAdminAccessAsync(
                request.Username,
                cancellationToken);

            if (!hasAdminAccess)
                throw new UnauthorizedAccessException("Only administrators can access admin sidebar menus.");

            return await endpointService.GetAccessibleMenuNamesAsync(
                request.Username,
                cancellationToken);
        }
    }
}
