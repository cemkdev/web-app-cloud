using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Products.Queries.GetQrCodeFromProduct
{
    public sealed class GetQrCodeFromProductQueryHandler(IProductService productService)
        : IRequestHandler<GetQrCodeFromProductQueryRequest, GetQrCodeFromProductQueryResponse>
    {
        public async Task<GetQrCodeFromProductQueryResponse> Handle(GetQrCodeFromProductQueryRequest request, CancellationToken cancellationToken)
        {
            byte[] qrCode = await productService.QrCodeFromProductAsync(request.ProductId, cancellationToken);

            return new GetQrCodeFromProductQueryResponse
            {
                QrCode = qrCode
            };
        }
    }
}
