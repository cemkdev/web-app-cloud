namespace WebAppAPI.Application.Features.Products.Commands.CreateProduct.DTOs
{
    public sealed class CreateProductDto
    {
        public required string Name { get; init; }
        public required int Stock { get; init; }
        public required float Price { get; init; }
        public required string Title { get; init; }
        public required string Description { get; init; }
    }
}
