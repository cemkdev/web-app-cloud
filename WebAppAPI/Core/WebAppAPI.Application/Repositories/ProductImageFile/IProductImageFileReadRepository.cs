using E = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IProductImageFileReadRepository : IReadRepository<E.ProductImageFile>
    {
        Task<E.ProductImageFile?> GetCurrentCoverImageAsync(Guid productId, bool tracking = true);
        Task<E.ProductImageFile?> GetByProductIdAndImageIdAsync(Guid productId, Guid imageId, bool tracking = true);
    }
}
