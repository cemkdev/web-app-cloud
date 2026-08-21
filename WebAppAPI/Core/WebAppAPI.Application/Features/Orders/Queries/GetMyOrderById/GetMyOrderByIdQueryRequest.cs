using MediatR;

namespace WebAppAPI.Application.Features.Orders.Queries.GetMyOrderById
{
    public sealed class GetMyOrderByIdQueryRequest : IRequest<GetMyOrderByIdQueryResponse>
    {
        public required string Id { get; init; }
    }
}
