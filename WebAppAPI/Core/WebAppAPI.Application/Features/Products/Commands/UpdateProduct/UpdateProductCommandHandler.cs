using MediatR;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Features.Products.Commands.UpdateProduct.DTOs;

namespace WebAppAPI.Application.Features.Products.Commands.UpdateProduct
{
    public sealed class UpdateProductCommandHandler(IProductService productService) : IRequestHandler<UpdateProductCommandRequest>
    {
        public async Task Handle(UpdateProductCommandRequest request, CancellationToken cancellationToken)
        {
            await productService.UpdateProductAsync(
            new UpdateProductDto
            {
                Id = request.Id,
                Name = request.Name,
                Stock = request.Stock,
                Price = request.Price,
                Title = request.Title,
                Description = request.Description
            },
            cancellationToken);
        }
    }
}
