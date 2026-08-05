using MediatR;

namespace WebAppAPI.Application.Features.Roles.Queries.GetRoles
{
    public sealed class GetRolesQueryRequest : IRequest<List<GetRolesQueryResponse>>;
}
