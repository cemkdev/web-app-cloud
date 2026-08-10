namespace WebAppAPI.Application.Features.Products.Commands.UpdateProduct.DTOs
{
    public sealed class UpdateProductDto
    {
        public required string Id { get; init; }
        public string? Name { get; init; }
        public int? Stock { get; init; }
        public float? Price { get; init; }
        public string? Title { get; init; }
        public string? Description { get; init; }
    }
}
