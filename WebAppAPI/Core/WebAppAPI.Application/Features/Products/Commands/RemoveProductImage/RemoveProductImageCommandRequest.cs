using MediatR;

namespace WebAppAPI.Application.Features.Products.Commands.RemoveProductImage
{
    public sealed class RemoveProductImageCommandRequest : IRequest
    {
        public required string ProductId { get; init; }
        public required string ImageId { get; init; }
    }
}
