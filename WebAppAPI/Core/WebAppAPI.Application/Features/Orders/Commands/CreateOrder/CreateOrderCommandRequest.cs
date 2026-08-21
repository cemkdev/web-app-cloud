using MediatR;

namespace WebAppAPI.Application.Features.Orders.Commands.CreateOrder
{
    public sealed class CreateOrderCommandRequest : IRequest
    {
        public required string Description { get; init; }
        public required string Address { get; init; }
    }
}
