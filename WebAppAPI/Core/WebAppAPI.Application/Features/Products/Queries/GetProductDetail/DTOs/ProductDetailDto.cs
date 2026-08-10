using WebAppAPI.Application.Features.Products.DTOs;

namespace WebAppAPI.Application.Features.Products.Queries.GetProductDetail.DTOs
{
    public sealed class ProductDetailDto
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public required int Stock { get; init; }
        public required float Price { get; init; }
        public required string Title { get; init; }
        public required string Description { get; init; }
        public float? Rating { get; init; }
        public required DateTime DateCreated { get; init; }
        public required DateTime DateUpdated { get; init; }
        public required IReadOnlyList<ProductImageDto> ProductImageFiles { get; init; }
    }
}
