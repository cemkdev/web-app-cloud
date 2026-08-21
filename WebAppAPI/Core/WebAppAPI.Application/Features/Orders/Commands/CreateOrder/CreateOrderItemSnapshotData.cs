namespace WebAppAPI.Application.Features.Orders.Commands.CreateOrder
{
    public sealed class CreateOrderItemSnapshotData
    {
        public Guid ProductId { get; init; }

        public required string Name { get; init; }
        public required string Title { get; init; }
        public required string Description { get; init; }
        public float? Rating { get; init; }

        public float UnitPrice { get; init; }
        public int Quantity { get; init; }
    }
}
