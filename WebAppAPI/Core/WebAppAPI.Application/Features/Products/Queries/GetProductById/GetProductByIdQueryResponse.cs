namespace WebAppAPI.Application.Features.Products.Queries.GetProductById
{
    public sealed class GetProductByIdQueryResponse
    {
        public required string Name { get; init; }
        public required int Stock { get; init; }
        public required float Price { get; init; }
        public required string Title { get; init; }
        public required string Description { get; init; }
        public float? Rating { get; init; }
    }
}
