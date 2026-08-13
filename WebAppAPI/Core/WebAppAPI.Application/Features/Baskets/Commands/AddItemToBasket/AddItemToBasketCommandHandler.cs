using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Baskets.Commands.AddItemToBasket
{
    public sealed class AddItemToBasketCommandHandler(IBasketService basketService) : IRequestHandler<AddItemToBasketCommandRequest>
    {
        public Task Handle(AddItemToBasketCommandRequest request, CancellationToken cancellationToken)
            => basketService.AddItemToBasketAsync(
                new AddBasketItemDto
                {
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                },
                cancellationToken);
    }
}
