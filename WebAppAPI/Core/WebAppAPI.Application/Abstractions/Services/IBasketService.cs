using WebAppAPI.Application.Features.Baskets.Commands.AddItemToBasket;
using WebAppAPI.Application.Features.Baskets.Commands.UpdateQuantity;
using WebAppAPI.Application.Features.Baskets.Queries.GetAllBasketItems;

namespace WebAppAPI.Application.Abstractions.Services
{
    public interface IBasketService
    {
        Task<IReadOnlyList<BasketItemListDto>> GetAllBasketItemsAsync(CancellationToken cancellationToken);
        Task AddItemToBasketAsync(AddBasketItemDto basketItem, CancellationToken cancellationToken);
        Task UpdateQuantityAsync(BasketItemQuantityUpdateDto basketItem, CancellationToken cancellationToken);
        Task RemoveBasketItemAsync(string basketItemId, CancellationToken cancellationToken);
    }
}
