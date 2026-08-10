using MediatR;

namespace WebAppAPI.Application.Features.Products.Commands.RemoveRangeProduct
{
    public sealed class RemoveRangeProductCommandRequest : IRequest
    {
        public required IReadOnlyCollection<string> ProductIds { get; init; }
    }
}
