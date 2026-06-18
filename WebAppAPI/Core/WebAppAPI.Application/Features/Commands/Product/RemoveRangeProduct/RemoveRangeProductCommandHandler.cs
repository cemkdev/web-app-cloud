using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Commands.Product.RemoveRangeProduct
{
    public class RemoveRangeProductCommandHandler : IRequestHandler<RemoveRangeProductCommandRequest, RemoveRangeProductCommandResponse>
    {
        readonly IProductService _productService;

        public RemoveRangeProductCommandHandler(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<RemoveRangeProductCommandResponse> Handle(RemoveRangeProductCommandRequest request, CancellationToken cancellationToken)
        {
            await _productService.RemoveRangeProductAsync(request.ProductIds);

            return new();
        }
    }
}
