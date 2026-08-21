using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IProductWriteRepository : IWriteRepository<Entities.Product>
    {
        Task<bool> TryDecreaseStockAsync(Guid productId, int quantity, CancellationToken cancellationToken);
    }
}
