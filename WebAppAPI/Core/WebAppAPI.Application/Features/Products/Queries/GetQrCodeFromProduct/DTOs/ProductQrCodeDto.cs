namespace WebAppAPI.Application.Features.Products.Queries.GetQrCodeFromProduct.DTOs
{
    public sealed class ProductQrCodeDto
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public required string Description { get; init; }
        public required float Price { get; init; }
        public required int Stock { get; init; }
        public required DateTime DateCreated { get; init; }
    }
}
