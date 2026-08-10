using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Products.Commands.RemoveProduct
{
    public sealed class RemoveProductCommandHandler(IProductService productService) : IRequestHandler<RemoveProductCommandRequest>
    {
        public Task Handle(RemoveProductCommandRequest request, CancellationToken cancellationToken)
            => productService.RemoveProductAsync(
                request.Id,
                cancellationToken);
    }
}
