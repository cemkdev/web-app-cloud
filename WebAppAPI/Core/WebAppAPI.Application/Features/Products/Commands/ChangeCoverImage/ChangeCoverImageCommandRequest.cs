using MediatR;

namespace WebAppAPI.Application.Features.Products.Commands.ChangeCoverImage
{
    public sealed class ChangeCoverImageCommandRequest : IRequest
    {
        public required string ProductId { get; init; }
        public required string ImageId { get; init; }
    }
}
