using MediatR;

namespace WebAppAPI.Application.Features.Endpoints.Queries.HasAccessToMenu
{
    public sealed class HasAccessToMenuQueryRequest : IRequest<HasAccessToMenuQueryResponse>
    {
        public required string Username { get; init; }
        public required string MenuName { get; init; }
    }
}
