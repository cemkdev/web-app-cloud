using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Persistence.Contexts;
using E = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Repositories
{
    public class ProductReadRepository(WebAppAPIDbContext context)
        : ReadRepository<E.Product>(context), IProductReadRepository
    {
        public Task<E.Product?> GetByIdWithImagesAsync(Guid id, bool tracking = false)
            => Query(tracking)
                .Include(p => p.ProductImageFiles)
                .FirstOrDefaultAsync(p => p.Id == id);

        public Task<List<E.Product>> GetByIdsWithImagesAsync(IEnumerable<Guid> ids, bool tracking = false)
            => Query(tracking)
                .Include(p => p.ProductImageFiles)
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();

        public async Task<(List<E.Product> Products, int TotalCount)> GetPagedWithImagesAsync(int page, int size)
        {
            IQueryable<E.Product> query = Query(false);
            int totalCount = await query.CountAsync();
            List<E.Product> products = await query
                .OrderByDescending(p => p.Price)
                .Skip(page * size)
                .Take(size)
                .Include(p => p.ProductImageFiles)
                .ToListAsync();

            return (products, totalCount);
        }
    }
}
