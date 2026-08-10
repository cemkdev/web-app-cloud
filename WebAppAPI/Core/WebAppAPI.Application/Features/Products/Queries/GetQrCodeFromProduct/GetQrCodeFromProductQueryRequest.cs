using MediatR;

namespace WebAppAPI.Application.Features.Products.Queries.GetQrCodeFromProduct
{
    public sealed class GetQrCodeFromProductQueryRequest : IRequest<GetQrCodeFromProductQueryResponse>
    {
        public required string ProductId { get; init; }
    }
}
