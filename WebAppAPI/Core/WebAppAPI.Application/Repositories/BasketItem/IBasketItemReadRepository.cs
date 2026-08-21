using WebAppAPI.Application.Features.Orders.Commands.CreateOrder;
using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IBasketItemReadRepository : IReadRepository<Entities.BasketItem>
    {
        Task<Entities.BasketItem?> GetByBasketIdAndProductIdAsync(Guid basketId, Guid productId, CancellationToken cancellationToken);
        Task<Entities.BasketItem?> GetForUpdateAsync(Guid basketItemId, Guid basketId, CancellationToken cancellationToken);
        Task<Entities.BasketItem?> GetForDeleteAsync(Guid basketItemId, Guid basketId, CancellationToken cancellationToken);
        Task<IReadOnlyList<CreateOrderBasketItemData>> GetOrderItemsByBasketIdAsync(Guid basketId, CancellationToken cancellationToken);
        Task<IReadOnlyList<CreateOrderItemSnapshotData>> GetOrderItemSnapshotsByBasketIdAsync(Guid basketId, CancellationToken cancellationToken);
    }
}
