using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Products.Commands.UploadProductImage
{
    public sealed class UploadProductImageCommandHandler(IProductService productService) : IRequestHandler<UploadProductImageCommandRequest>
    {
        public Task Handle(UploadProductImageCommandRequest request, CancellationToken cancellationToken)
            => productService.UploadProductImagesAsync(request.Id, request.Files, cancellationToken);
    }
}
