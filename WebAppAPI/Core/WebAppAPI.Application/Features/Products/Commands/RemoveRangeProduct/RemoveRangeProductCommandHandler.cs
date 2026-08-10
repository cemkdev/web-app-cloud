using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Products.Commands.RemoveRangeProduct
{
    public sealed class RemoveRangeProductCommandHandler(IProductService productService) : IRequestHandler<RemoveRangeProductCommandRequest>
    {
        public Task Handle(RemoveRangeProductCommandRequest request, CancellationToken cancellationToken)
            => productService.RemoveRangeProductAsync(
                request.ProductIds,
                cancellationToken);
    }
}
