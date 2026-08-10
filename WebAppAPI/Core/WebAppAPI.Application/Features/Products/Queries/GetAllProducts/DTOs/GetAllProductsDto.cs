namespace WebAppAPI.Application.Features.Products.Queries.GetAllProducts.DTOs
{
    public sealed class GetAllProductsDto
    {
        public required int TotalProductCount { get; init; }
        public required IReadOnlyList<ProductListItemDto> Products { get; init; }
    }
}
