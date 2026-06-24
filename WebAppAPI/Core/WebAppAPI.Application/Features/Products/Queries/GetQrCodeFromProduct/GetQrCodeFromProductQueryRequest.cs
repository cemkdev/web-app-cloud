using MediatR;

namespace WebAppAPI.Application.Features.Products.Queries.GetQrCodeFromProduct
{
    public class GetQrCodeFromProductQueryRequest : IRequest<GetQrCodeFromProductQueryResponse>
    {
        public string ProductId { get; set; }
    }
}
