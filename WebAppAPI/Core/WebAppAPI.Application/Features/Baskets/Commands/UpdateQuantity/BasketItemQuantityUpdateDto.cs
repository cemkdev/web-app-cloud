namespace WebAppAPI.Application.Features.Baskets.Commands.UpdateQuantity
{
    public sealed class BasketItemQuantityUpdateDto
    {
        public required string BasketItemId { get; init; }
        public int Quantity { get; init; }
    }
}
