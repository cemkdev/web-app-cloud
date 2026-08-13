namespace WebAppAPI.Application.Features.Baskets.Queries.GetAllBasketItems
{
    public sealed class BasketItemListDto
    {
        public Guid BasketItemId { get; init; }
        public Guid ProductId { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public int Stock { get; init; }
        public float Price { get; init; }
        public int Quantity { get; init; }
        public BasketItemImageDto? ProductImageFile { get; init; }
    }
}
