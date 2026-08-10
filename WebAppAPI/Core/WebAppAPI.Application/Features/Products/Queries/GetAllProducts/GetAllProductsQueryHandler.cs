using MediatR;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Features.Products.Queries.GetAllProducts.DTOs;

namespace WebAppAPI.Application.Features.Products.Queries.GetAllProducts
{
    public sealed class GetAllProductsQueryHandler(IProductService productService) : IRequestHandler<GetAllProductsQueryRequest, GetAllProductsQueryResponse>
    {
        public async Task<GetAllProductsQueryResponse> Handle(GetAllProductsQueryRequest request, CancellationToken cancellationToken)
        {
            GetAllProductsDto result = await productService.GetAllProductsAsync(
                request.Page,
                request.Size,
                cancellationToken);

            return new GetAllProductsQueryResponse
            {
                TotalProductCount = result.TotalProductCount,
                Products = result.Products
            };
        }
    }
}
