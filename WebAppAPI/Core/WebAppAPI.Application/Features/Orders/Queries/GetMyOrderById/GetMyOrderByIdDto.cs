namespace WebAppAPI.Application.Features.Orders.Queries.GetMyOrderById
{
    public sealed class GetMyOrderByIdDto
    {
        public required Guid Id { get; init; }
        public required string OrderCode { get; init; }
        public required string Address { get; init; }
        public required string Description { get; init; }
        public required DateTime DateCreated { get; init; }
        public required int StatusId { get; init; }

        public IReadOnlyList<MyOrderDetailItemDto> OrderBasketItems { get; set; } = [];
    }
}
