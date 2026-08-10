namespace WebAppAPI.Application.Features.Products.Queries.GetProductById.DTOs
{
    public sealed class ProductByIdDto
    {
        public required string Name { get; init; }
        public required int Stock { get; init; }
        public required float Price { get; init; }
        public required string Title { get; init; }
        public required string Description { get; init; }
        public float? Rating { get; init; }
    }
}
