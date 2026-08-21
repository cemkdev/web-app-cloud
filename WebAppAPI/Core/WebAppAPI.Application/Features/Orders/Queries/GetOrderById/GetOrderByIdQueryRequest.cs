using MediatR;

namespace WebAppAPI.Application.Features.Orders.Queries.GetOrderById
{
    public sealed class GetOrderByIdQueryRequest : IRequest<GetOrderByIdQueryResponse>
    {
        public required string Id { get; init; }
    }
}
