using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Products.Commands.RemoveProductImage
{
    public sealed class RemoveProductImageCommandHandler(IProductService productService) : IRequestHandler<RemoveProductImageCommandRequest>
    {
        public Task Handle(RemoveProductImageCommandRequest request, CancellationToken cancellationToken)
            => productService.RemoveProductImageAsync(
                request.ProductId,
                request.ImageId,
                cancellationToken);
    }
}
