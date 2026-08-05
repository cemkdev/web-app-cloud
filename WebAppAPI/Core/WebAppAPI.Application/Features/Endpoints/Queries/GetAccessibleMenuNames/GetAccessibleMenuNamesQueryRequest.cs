using MediatR;

namespace WebAppAPI.Application.Features.Endpoints.Queries.GetAccessibleMenuNames
{
    public sealed class GetAccessibleMenuNamesQueryRequest : IRequest<List<string>>
    {
        public required string Username { get; init; }
    }
}
