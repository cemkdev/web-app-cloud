using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Persistence.Contexts;
using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Repositories
{
    public sealed class ProductWriteRepository(WebAppAPIDbContext context) : WriteRepository<Entities.Product>(context), IProductWriteRepository
    {
        public async Task<bool> TryDecreaseStockAsync(Guid productId, int quantity, CancellationToken cancellationToken)
        {
            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

            DateTime updatedAt = DateTime.UtcNow;

            int affectedRows = await Set
                .Where(product =>
                    product.Id == productId &&
                    product.Stock >= quantity)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            product => product.Stock,
                            product => product.Stock - quantity)
                        .SetProperty(
                            product => product.DateUpdated,
                            updatedAt),
                    cancellationToken);

            return affectedRows == 1;
        }
    }
}
