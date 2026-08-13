using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Baskets.Commands.UpdateQuantity
{
    public sealed class UpdateQuantityCommandHandler(IBasketService basketService) : IRequestHandler<UpdateQuantityCommandRequest>
    {
        public Task Handle(UpdateQuantityCommandRequest request, CancellationToken cancellationToken)
            => basketService.UpdateQuantityAsync(
                new BasketItemQuantityUpdateDto
                {
                    BasketItemId = request.BasketItemId,
                    Quantity = request.Quantity
                },
                cancellationToken);
    }
}
