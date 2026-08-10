namespace WebAppAPI.Application.Features.Products.Queries.GetProductImages
{
    public sealed class GetProductImagesQueryResponse
    {
        public required Guid Id { get; init; }
        public required string Path { get; init; }
        public required string FileName { get; init; }
        public required bool CoverImage { get; init; }
    }
}
