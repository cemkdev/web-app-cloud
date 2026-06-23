using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Persistence.Contexts;
using E = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Repositories
{
    public class BasketReadRepository(WebAppAPIDbContext context)
        : ReadRepository<E.Basket>(context), IBasketReadRepository
    {
        public Task<E.Basket?> GetWithItemsAndProductImagesAsync(Guid basketId, bool tracking = false)
            => Query(tracking)
                .Include(b => b.BasketItems)
                    .ThenInclude(bi => bi.Product)
                        .ThenInclude(p => p.ProductImageFiles)
                .FirstOrDefaultAsync(b => b.Id == basketId);
    }
}
