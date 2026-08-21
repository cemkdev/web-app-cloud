namespace WebAppAPI.Application.Features.Orders.Queries.GetOrderById
{
    public sealed class GetOrderByIdQueryResponse
    {
        public required Guid Id { get; init; }
        public required string OrderCode { get; init; }
        public required string Address { get; init; }
        public required string Description { get; init; }
        public required DateTime DateCreated { get; init; }
        public required int StatusId { get; init; }
        public required IReadOnlyList<OrderDetailItemDto> OrderBasketItems { get; init; }
    }
}
