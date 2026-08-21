namespace WebAppAPI.Application.Features.Orders.Queries.GetAllOrders
{
    public sealed class OrderListItemDto
    {
        public required Guid Id { get; init; }
        public required string OrderCode { get; init; }
        public required string CustomerName { get; init; }
        public required float TotalPrice { get; set; }
        public required DateTime DateCreated { get; init; }
        public required int StatusId { get; init; }
    }
}
