using MediatR;

namespace WebAppAPI.Application.Features.Products.Commands.CreateProduct
{
    public sealed class CreateProductCommandRequest : IRequest<CreateProductCommandResponse>
    {
        public required string Name { get; init; }
        public required int Stock { get; init; }
        public required float Price { get; init; }
        public required string Title { get; init; }
        public required string Description { get; init; }
    }
}
