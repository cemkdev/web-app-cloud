using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Persistence.Contexts;
using E = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Repositories
{
    public class BasketItemReadRepository(WebAppAPIDbContext context)
        : ReadRepository<E.BasketItem>(context), IBasketItemReadRepository
    {
        public Task<E.BasketItem?> GetByBasketAndProductAsync(Guid basketId, Guid productId, bool tracking = true)
            => Query(tracking)
                .FirstOrDefaultAsync(bi => bi.BasketId == basketId && bi.ProductId == productId);

        public Task<E.BasketItem?> GetByIdAndBasketAsync(Guid basketItemId, Guid basketId, bool tracking = true)
            => Query(tracking)
                .Include(bi => bi.Product)
                .FirstOrDefaultAsync(bi => bi.Id == basketItemId && bi.BasketId == basketId);

        public Task<List<E.BasketItem>> GetByBasketIdAsync(Guid basketId, bool tracking = false)
            => Query(tracking)
                .Where(bi => bi.BasketId == basketId)
                .ToListAsync();

        public Task<bool> AnyByBasketIdAsync(Guid basketId)
            => Query(false).AnyAsync(bi => bi.BasketId == basketId);
    }
}
