using WebAppAPI.Application.Features.Products.Queries.GetAllProducts.DTOs;

namespace WebAppAPI.Application.Features.Products.Queries.GetAllProducts
{
    public sealed class GetAllProductsQueryResponse
    {
        public required int TotalProductCount { get; init; }
        public required IReadOnlyList<ProductListItemDto> Products { get; init; }
    }
}
