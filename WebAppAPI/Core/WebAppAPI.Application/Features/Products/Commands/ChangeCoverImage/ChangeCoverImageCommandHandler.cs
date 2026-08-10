using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Products.Commands.ChangeCoverImage
{
    public sealed class ChangeCoverImageCommandHandler(IProductService productService) : IRequestHandler<ChangeCoverImageCommandRequest>
    {
        public Task Handle(ChangeCoverImageCommandRequest request, CancellationToken cancellationToken)
            => productService.ChangeCoverImageAsync(
                request.ProductId,
                request.ImageId,
                cancellationToken);
    }
}
