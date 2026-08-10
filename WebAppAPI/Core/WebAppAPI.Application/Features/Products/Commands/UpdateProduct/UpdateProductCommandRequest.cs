using MediatR;

namespace WebAppAPI.Application.Features.Products.Commands.UpdateProduct
{
    public sealed class UpdateProductCommandRequest : IRequest
    {
        public required string Id { get; init; }
        public string? Name { get; init; }
        public int? Stock { get; init; }
        public float? Price { get; init; }
        public string? Title { get; init; }
        public string? Description { get; init; }
    }
}
