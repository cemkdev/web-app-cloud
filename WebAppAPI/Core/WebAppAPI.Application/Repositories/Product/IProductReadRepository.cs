using E = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IProductReadRepository : IReadRepository<E.Product>
    {
        Task<E.Product?> GetByIdWithImagesAsync(Guid id, bool tracking = false);
        Task<List<E.Product>> GetByIdsWithImagesAsync(IEnumerable<Guid> ids, bool tracking = false);
        Task<(List<E.Product> Products, int TotalCount)> GetPagedWithImagesAsync(int page, int size);
    }
}
