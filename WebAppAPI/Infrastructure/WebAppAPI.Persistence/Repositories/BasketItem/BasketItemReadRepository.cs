using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Persistence.Contexts;
using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Repositories
{
    public class BasketItemReadRepository(WebAppAPIDbContext context)
        : ReadRepository<Entities.BasketItem>(context), IBasketItemReadRepository
    {
        public Task<Entities.BasketItem?> GetByBasketIdAndProductIdAsync(Guid basketId, Guid productId, CancellationToken cancellationToken)
            => Query(tracking: true)
                .FirstOrDefaultAsync(
                    basketItem =>
                        basketItem.BasketId == basketId &&
                        basketItem.ProductId == productId,
                    cancellationToken);

        public Task<Entities.BasketItem?> GetForUpdateAsync(Guid basketItemId, Guid basketId, CancellationToken cancellationToken)
            => Query(tracking: true)
                .Include(basketItem => basketItem.Product)
                .FirstOrDefaultAsync(
                    basketItem =>
                        basketItem.Id == basketItemId &&
                        basketItem.BasketId == basketId,
                    cancellationToken);

        public Task<Entities.BasketItem?> GetForDeleteAsync(Guid basketItemId, Guid basketId, CancellationToken cancellationToken)
            => Query(tracking: true)
                .FirstOrDefaultAsync(
                    basketItem =>
                        basketItem.Id == basketItemId &&
                        basketItem.BasketId == basketId,
                    cancellationToken);




        public Task<List<Entities.BasketItem>> GetByBasketIdAsync(Guid basketId, bool tracking = false)
            => Query(tracking)
                .Where(bi => bi.BasketId == basketId)
                .ToListAsync();

        public Task<bool> AnyByBasketIdAsync(Guid basketId)
            => Query(false).AnyAsync(bi => bi.BasketId == basketId);
    }
}
