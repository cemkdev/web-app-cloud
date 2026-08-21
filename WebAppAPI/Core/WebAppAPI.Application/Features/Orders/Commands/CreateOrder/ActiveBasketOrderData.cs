namespace WebAppAPI.Application.Features.Orders.Commands.CreateOrder
{
    public sealed class ActiveBasketOrderData
    {
        public required Guid BasketId { get; init; }
        public string? Recipient { get; init; }
        public required string FirstName { get; init; }
    }
}
