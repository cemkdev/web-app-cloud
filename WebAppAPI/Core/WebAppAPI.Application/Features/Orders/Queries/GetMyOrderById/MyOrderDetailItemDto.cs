namespace WebAppAPI.Application.Features.Orders.Queries.GetMyOrderById
{
    public sealed class MyOrderDetailItemDto
    {
        public required string Name { get; init; }
        public required string Title { get; init; }
        public required string Description { get; init; }
        public required float Price { get; init; }
        public required int Quantity { get; init; }
        public required float? Rating { get; init; }
        public required bool IsProductDeleted { get; init; }

        public MyOrderProductImageDto? OrderProductImageFile { get; set; }
    }
}
