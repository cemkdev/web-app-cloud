using MediatR;

namespace WebAppAPI.Application.Features.Baskets.Commands.UpdateQuantity
{
    public sealed class UpdateQuantityCommandRequest : IRequest
    {
        public required string BasketItemId { get; init; }
        public int Quantity { get; init; }
    }
}
