using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Products.Commands.ChangeCoverImage
{
    public class ChangeCoverImageCommandHandler : IRequestHandler<ChangeCoverImageCommandRequest, ChangeCoverImageCommandResponse>
    {
        readonly IProductService _productService;

        public ChangeCoverImageCommandHandler(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<ChangeCoverImageCommandResponse> Handle(ChangeCoverImageCommandRequest request, CancellationToken cancellationToken)
        {
            await _productService.ChangeCoverImageAsync(request.ProductId, request.ImageId);

            return new();
        }
    }
}
