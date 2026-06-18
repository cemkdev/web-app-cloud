using MediatR;
using WebAppAPI.Application.Abstractions.Hubs;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Commands.Product.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommandRequest, CreateProductCommandResponse>
    {
        readonly IProductService _productService;
        readonly IProductHubService _productHubService;

        public CreateProductCommandHandler(IProductService productService, IProductHubService productHubService)
        {
            _productService = productService;
            _productHubService = productHubService;
        }

        public async Task<CreateProductCommandResponse> Handle(CreateProductCommandRequest request, CancellationToken cancellationToken)
        {
            await _productService.CreateProductAsync(new()
            {
                Name = request.Name,
                Stock = request.Stock,
                Price = request.Price,
                Title = request.Title,
                Description = request.Description
            });

            await _productHubService.ProductAddedMessageAsync($"'{request.Name}' has been added.");

            return new();
        }
    }
}
