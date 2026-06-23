using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Persistence.Contexts;
using E = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Repositories
{
    public class ProductImageFileReadRepository(WebAppAPIDbContext context)
        : ReadRepository<E.ProductImageFile>(context), IProductImageFileReadRepository
    {
        public Task<E.ProductImageFile?> GetCurrentCoverImageAsync(Guid productId, bool tracking = true)
            => Query(tracking)
                .Include(p => p.Product)
                .Where(p => p.CoverImage && p.Product.Any(prod => prod.Id == productId))
                .FirstOrDefaultAsync();

        public Task<E.ProductImageFile?> GetByProductIdAndImageIdAsync(Guid productId, Guid imageId, bool tracking = true)
            => Query(tracking)
                .Include(p => p.Product)
                .Where(p => p.Id == imageId && p.Product.Any(prod => prod.Id == productId))
                .FirstOrDefaultAsync();
    }
}
