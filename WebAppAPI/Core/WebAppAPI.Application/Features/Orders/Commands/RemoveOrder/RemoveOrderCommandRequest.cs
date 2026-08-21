using MediatR;

namespace WebAppAPI.Application.Features.Orders.Commands.RemoveOrder
{
    public sealed class RemoveOrderCommandRequest : IRequest
    {
        public required string Id { get; init; }
    }
}
