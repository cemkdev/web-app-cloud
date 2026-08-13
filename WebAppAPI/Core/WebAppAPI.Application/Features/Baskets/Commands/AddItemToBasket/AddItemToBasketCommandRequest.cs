using MediatR;

namespace WebAppAPI.Application.Features.Baskets.Commands.AddItemToBasket
{
    public sealed class AddItemToBasketCommandRequest : IRequest
    {
        public required string ProductId { get; init; }
        public int Quantity { get; init; }
    }
}
