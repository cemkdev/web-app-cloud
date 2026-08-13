using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Baskets.Commands.RemoveBasketItem
{
    public sealed class RemoveBasketItemCommandHandler(IBasketService basketService) : IRequestHandler<RemoveBasketItemCommandRequest>
    {
        public Task Handle(RemoveBasketItemCommandRequest request, CancellationToken cancellationToken)
            => basketService.RemoveBasketItemAsync(request.BasketItemId, cancellationToken);
    }
}
