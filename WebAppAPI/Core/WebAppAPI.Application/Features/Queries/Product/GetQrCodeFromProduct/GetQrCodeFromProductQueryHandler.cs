using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Queries.Product.GetQrCodeFromProduct
{
    public class GetQrCodeFromProductQueryHandler : IRequestHandler<GetQrCodeFromProductQueryRequest, GetQrCodeFromProductQueryResponse>
    {
        readonly IProductService _productService;

        public GetQrCodeFromProductQueryHandler(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<GetQrCodeFromProductQueryResponse> Handle(GetQrCodeFromProductQueryRequest request, CancellationToken cancellationToken)
        {
            byte[] qrCode = await _productService.QrCodeFromProductAsync(request.ProductId);

            return new()
            {
                QrCode = qrCode
            };
        }
    }
}
