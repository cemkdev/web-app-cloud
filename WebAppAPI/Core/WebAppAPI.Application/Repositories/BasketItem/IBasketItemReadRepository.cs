using E = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IBasketItemReadRepository : IReadRepository<E.BasketItem>
    {
        Task<E.BasketItem?> GetByBasketAndProductAsync(Guid basketId, Guid productId, bool tracking = true);
        Task<E.BasketItem?> GetByIdAndBasketAsync(Guid basketItemId, Guid basketId, bool tracking = true);
        Task<List<E.BasketItem>> GetByBasketIdAsync(Guid basketId, bool tracking = false);
        Task<bool> AnyByBasketIdAsync(Guid basketId);
    }
}
