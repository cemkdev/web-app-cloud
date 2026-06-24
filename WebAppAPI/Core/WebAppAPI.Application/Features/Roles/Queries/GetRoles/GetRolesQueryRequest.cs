using MediatR;

namespace WebAppAPI.Application.Features.Roles.Queries.GetRoles
{
    public class GetRolesQueryRequest : IRequest<List<GetRolesQueryResponse>>
    {
    }
}
