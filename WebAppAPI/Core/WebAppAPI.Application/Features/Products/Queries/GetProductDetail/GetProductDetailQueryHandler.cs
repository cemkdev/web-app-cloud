using MediatR;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Features.Products.Queries.GetProductDetail.DTOs;

namespace WebAppAPI.Application.Features.Products.Queries.GetProductDetail
{
    public sealed class GetProductDetailQueryHandler(IProductService productService) : IRequestHandler<GetProductDetailQueryRequest, GetProductDetailQueryResponse>
    {
        public async Task<GetProductDetailQueryResponse> Handle(GetProductDetailQueryRequest request, CancellationToken cancellationToken)
        {
            ProductDetailDto product = await productService.GetProductDetailAsync(
                request.Id,
                cancellationToken);

            return new GetProductDetailQueryResponse
            {
                Id = product.Id,
                Name = product.Name,
                Stock = product.Stock,
                Price = product.Price,
                Title = product.Title,
                Description = product.Description,
                Rating = product.Rating,
                DateCreated = product.DateCreated,
                DateUpdated = product.DateUpdated,
                ProductImageFiles = product.ProductImageFiles
            };
        }
    }
}
