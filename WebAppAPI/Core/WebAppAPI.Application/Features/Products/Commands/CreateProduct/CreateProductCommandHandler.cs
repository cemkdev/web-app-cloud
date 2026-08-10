using MediatR;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Features.Products.Commands.CreateProduct.DTOs;
using WebAppAPI.Application.Features.Products.Notifications;

namespace WebAppAPI.Application.Features.Products.Commands.CreateProduct
{
    public sealed class CreateProductCommandHandler(IProductService productService, IMediator mediator)
        : IRequestHandler<CreateProductCommandRequest, CreateProductCommandResponse>
    {
        public async Task<CreateProductCommandResponse> Handle(CreateProductCommandRequest request, CancellationToken cancellationToken)
        {
            Guid productId = await productService.CreateProductAsync(
                new CreateProductDto
                {
                    Name = request.Name,
                    Stock = request.Stock,
                    Price = request.Price,
                    Title = request.Title,
                    Description = request.Description
                },
                cancellationToken);

            await mediator.Publish(
                new ProductCreatedNotification
                {
                    ProductId = productId,
                    ProductName = request.Name
                },
                cancellationToken);

            return new CreateProductCommandResponse
            {
                Id = productId
            };
        }
    }
}
