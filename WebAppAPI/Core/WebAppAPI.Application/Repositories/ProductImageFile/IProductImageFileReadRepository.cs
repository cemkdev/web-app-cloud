using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IProductImageFileReadRepository : IReadRepository<Entities.ProductImageFile>
    {
        Task<Entities.ProductImageFile?> GetCoverByProductIdAsync(Guid productId, CancellationToken cancellationToken, bool tracking = false);
        Task<Entities.ProductImageFile?> GetByIdForProductAsync(Guid productId, Guid imageId, CancellationToken cancellationToken, bool tracking = false);
        Task<List<Entities.ProductImageFile>> GetByProductIdAndStorageAsync(Guid productId, string storageName, CancellationToken cancellationToken);
    }
}
