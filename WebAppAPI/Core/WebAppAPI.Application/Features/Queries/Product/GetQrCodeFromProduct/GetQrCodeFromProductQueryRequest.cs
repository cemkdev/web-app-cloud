using MediatR;

namespace WebAppAPI.Application.Features.Queries.Product.GetQrCodeFromProduct
{
    public class GetQrCodeFromProductQueryRequest : IRequest<GetQrCodeFromProductQueryResponse>
    {
        public string ProductId { get; set; }
    }
}
