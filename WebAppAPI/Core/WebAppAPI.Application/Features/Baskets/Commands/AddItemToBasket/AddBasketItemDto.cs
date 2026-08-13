namespace WebAppAPI.Application.Features.Baskets.Commands.AddItemToBasket
{
    public sealed class AddBasketItemDto
    {
        public required string ProductId { get; init; }
        public int Quantity { get; init; }
    }
}
