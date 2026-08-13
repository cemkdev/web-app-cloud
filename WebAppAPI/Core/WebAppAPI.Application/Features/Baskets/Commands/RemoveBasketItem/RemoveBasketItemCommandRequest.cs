using MediatR;

namespace WebAppAPI.Application.Features.Baskets.Commands.RemoveBasketItem
{
    public sealed class RemoveBasketItemCommandRequest : IRequest
    {
        public required string BasketItemId { get; init; }
    }
}
