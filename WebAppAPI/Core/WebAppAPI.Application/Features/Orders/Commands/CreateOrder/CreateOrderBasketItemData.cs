namespace WebAppAPI.Application.Features.Orders.Commands.CreateOrder
{
    public sealed class CreateOrderBasketItemData
    {
        public Guid ProductId { get; init; }
        public int Quantity { get; init; }
    }
}
