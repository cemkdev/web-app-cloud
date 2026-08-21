using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Features.Orders.Commands.CreateOrder;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Persistence.Contexts;
using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Repositories
{
    public class BasketItemReadRepository(WebAppAPIDbContext context)
        : ReadRepository<Entities.BasketItem>(context), IBasketItemReadRepository
    {
        public Task<Entities.BasketItem?> GetByBasketIdAndProductIdAsync(Guid basketId, Guid productId, CancellationToken cancellationToken)
            => Query(tracking: true)
                .FirstOrDefaultAsync(
                    basketItem =>
                        basketItem.BasketId == basketId &&
                        basketItem.ProductId == productId,
                    cancellationToken);

        public Task<Entities.BasketItem?> GetForUpdateAsync(Guid basketItemId, Guid basketId, CancellationToken cancellationToken)
            => Query(tracking: true)
                .Include(basketItem => basketItem.Product)
                .FirstOrDefaultAsync(
                    basketItem =>
                        basketItem.Id == basketItemId &&
                        basketItem.BasketId == basketId,
                    cancellationToken);

        public Task<Entities.BasketItem?> GetForDeleteAsync(Guid basketItemId, Guid basketId, CancellationToken cancellationToken)
            => Query(tracking: true)
                .FirstOrDefaultAsync(
                    basketItem =>
                        basketItem.Id == basketItemId &&
                        basketItem.BasketId == basketId,
                    cancellationToken);

        public async Task<IReadOnlyList<CreateOrderBasketItemData>> GetOrderItemsByBasketIdAsync(Guid basketId, CancellationToken cancellationToken)
            => await Query(tracking: false)
                .Where(basketItem => basketItem.BasketId == basketId)
                .OrderBy(basketItem => basketItem.ProductId)
                .Select(basketItem => new CreateOrderBasketItemData
                {
                    ProductId = basketItem.ProductId,
                    Quantity = basketItem.Quantity
                })
                .ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<CreateOrderItemSnapshotData>> GetOrderItemSnapshotsByBasketIdAsync(Guid basketId, CancellationToken cancellationToken)
            => await Query(tracking: false)
                .Where(basketItem => basketItem.BasketId == basketId)
                .OrderBy(basketItem => basketItem.ProductId)
                .Select(basketItem => new CreateOrderItemSnapshotData
                {
                    ProductId = basketItem.ProductId,
                    Name = basketItem.Product.Name,
                    Title = basketItem.Product.Title,
                    Description = basketItem.Product.Description,
                    Rating = basketItem.Product.Rating,
                    UnitPrice = basketItem.Product.Price,
                    Quantity = basketItem.Quantity
                })
                .ToListAsync(cancellationToken);
    }
}
