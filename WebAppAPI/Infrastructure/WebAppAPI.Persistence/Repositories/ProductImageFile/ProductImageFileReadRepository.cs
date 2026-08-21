using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Persistence.Contexts;
using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Repositories
{
    public class ProductImageFileReadRepository(WebAppAPIDbContext context)
        : ReadRepository<Entities.ProductImageFile>(context), IProductImageFileReadRepository
    {
        public Task<Entities.ProductImageFile?> GetCoverByProductIdAsync(Guid productId, CancellationToken cancellationToken, bool tracking = false)
            => Query(tracking)
                .FirstOrDefaultAsync(
                    image =>
                        image.CoverImage &&
                        image.Product.Any(product => product.Id == productId),
                    cancellationToken);

        public async Task<IReadOnlyDictionary<Guid, Entities.ProductImageFile>> GetCoversByProductIdsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken)
        {
            if (productIds.Count == 0)
                return new Dictionary<Guid, Entities.ProductImageFile>();

            Guid[] ids = productIds.ToArray();

            var covers = await Query(tracking: false)
                .Where(image => image.CoverImage)
                .SelectMany(
                    image => image.Product.Where(product => ids.Contains(product.Id)),
                    (image, product) => new
                    {
                        ProductId = product.Id,
                        Image = image
                    })
                .ToListAsync(cancellationToken);

            return covers.ToDictionary(
                item => item.ProductId,
                item => item.Image);
        }

        public Task<Entities.ProductImageFile?> GetByIdForProductAsync(Guid productId, Guid imageId, CancellationToken cancellationToken, bool tracking = false)
            => Query(tracking)
                .FirstOrDefaultAsync(
                    image =>
                        image.Id == imageId &&
                        image.Product.Any(product => product.Id == productId),
                    cancellationToken);

        public Task<List<Entities.ProductImageFile>> GetByProductIdAndStorageAsync(Guid productId, string storageName, CancellationToken cancellationToken)
            => Query(tracking: false)
                .Where(image =>
                    image.Storage == storageName &&
                    image.Product.Any(product => product.Id == productId))
                .OrderByDescending(image => image.CoverImage)
                .ThenBy(image => image.Id)
                .ToListAsync(cancellationToken);
    }
}
