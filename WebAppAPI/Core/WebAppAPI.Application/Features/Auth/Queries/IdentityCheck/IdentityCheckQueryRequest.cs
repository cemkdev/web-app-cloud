using MediatR;

namespace WebAppAPI.Application.Features.Auth.Queries.IdentityCheck
{
    public sealed class IdentityCheckQueryRequest : IRequest<IdentityCheckQueryResponse>
    {
    }
}
