using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Commands.Product.UpdateProduct
{
    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommandRequest, UpdateProductCommandResponse>
    {
        readonly IProductService _productService;

        public UpdateProductCommandHandler(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<UpdateProductCommandResponse> Handle(UpdateProductCommandRequest request, CancellationToken cancellationToken)
        {
            await _productService.UpdateProductAsync(new()
            {
                Id = request.Id,
                Name = request.Name,
                Stock = request.Stock,
                Price = request.Price,
                Title = request.Title,
                Description = request.Description
            });

            return new();
        }
    }
}
