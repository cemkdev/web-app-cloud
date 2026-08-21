namespace WebAppAPI.Application.Features.Orders.Queries.GetMyOrderById
{
    public sealed class MyOrderProductImageDto
    {
        public required Guid ProductImageFileId { get; init; }
        public required string FileName { get; init; }
        public required string Path { get; set; }
    }
}
