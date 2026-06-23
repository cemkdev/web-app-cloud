using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Persistence.Contexts;
using E = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Repositories
{
    public class OrderReadRepository(WebAppAPIDbContext context)
        : ReadRepository<E.Order>(context), IOrderReadRepository
    {
        public async Task<(List<E.Order> Orders, int TotalCount)> GetPagedWithBasketSummaryAsync(int page, int size)
        {
            IQueryable<E.Order> query = Query(false)
                .Include(o => o.Basket)
                    .ThenInclude(b => b.BasketItems)
                        .ThenInclude(bi => bi.Product)
                .Include(o => o.Basket.User);

            int totalCount = await query.CountAsync();
            List<E.Order> orders = await query
                .OrderBy(o => o.DateCreated)
                .Skip(page * size)
                .Take(size)
                .ToListAsync();

            return (orders, totalCount);
        }

        public Task<E.Order?> GetDetailByIdAsync(Guid id, bool tracking = false)
            => Query(tracking)
                .Include(o => o.Basket)
                    .ThenInclude(b => b.BasketItems)
                        .ThenInclude(bi => bi.Product)
                            .ThenInclude(p => p.ProductImageFiles)
                .FirstOrDefaultAsync(o => o.Id == id);

        public Task<E.Order?> GetWithBasketUserAsync(Guid id, bool tracking = false)
            => Query(tracking)
                .Include(o => o.Basket)
                    .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(o => o.Id == id);

        public Task<bool> HasOrderForBasketAsync(Guid basketId)
            => Query(false).AnyAsync(o => o.Id == basketId);
    }
}
