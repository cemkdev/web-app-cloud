namespace WebAppAPI.Application.Features.Products.Queries.GetQrCodeFromProduct
{
    public sealed class GetQrCodeFromProductQueryResponse
    {
        public required byte[] QrCode { get; init; }
    }
}
