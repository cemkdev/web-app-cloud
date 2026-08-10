using MediatR;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Features.Products.Queries.GetProductById.DTOs;

namespace WebAppAPI.Application.Features.Products.Queries.GetProductById
{
    public sealed class GetProductByIdQueryHandler(IProductService productService) : IRequestHandler<GetProductByIdQueryRequest, GetProductByIdQueryResponse>
    {
        public async Task<GetProductByIdQueryResponse> Handle(GetProductByIdQueryRequest request, CancellationToken cancellationToken)
        {
            ProductByIdDto product = await productService.GetProductByIdAsync(
                request.Id,
                cancellationToken);

            return new GetProductByIdQueryResponse
            {
                Name = product.Name,
                Stock = product.Stock,
                Price = product.Price,
                Title = product.Title,
                Description = product.Description,
                Rating = product.Rating
            };
        }
    }
}
