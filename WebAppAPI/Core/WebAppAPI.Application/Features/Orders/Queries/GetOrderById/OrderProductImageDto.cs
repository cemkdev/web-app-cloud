namespace WebAppAPI.Application.Features.Orders.Queries.GetOrderById
{
    public sealed class OrderProductImageDto
    {
        public required Guid ProductImageFileId { get; init; }
        public required string FileName { get; init; }
        public required string Path { get; set; }
    }
}
