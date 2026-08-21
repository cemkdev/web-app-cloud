namespace WebAppAPI.Application.Features.Orders.Queries.GetOrderById
{
    public sealed class OrderDetailItemDto
    {
        public required string Name { get; init; }
        public required string Title { get; init; }
        public required string Description { get; init; }
        public required float Price { get; init; }
        public required int Quantity { get; init; }
        public required float? Rating { get; init; }
        public required bool IsProductDeleted { get; init; }

        public OrderProductImageDto? OrderProductImageFile { get; set; }
    }
}
