namespace WebAppAPI.Application.Features.Baskets.Queries.GetAllBasketItems
{
    public sealed class BasketItemImageDto
    {
        public Guid ProductImageFileId { get; init; }
        public required string FileName { get; init; }
        public required string Path { get; init; }
    }
}
