using MediatR;

namespace WebAppAPI.Application.Features.Orders.Commands.RemoveRangeOrder
{
    public sealed class RemoveRangeOrderCommandRequest : IRequest
    {
        public required IReadOnlyCollection<string> OrderIds { get; init; }
    }
}
