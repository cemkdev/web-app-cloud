using WebAppAPI.Application.Features.Baskets.Queries.GetAllBasketItems;
using WebAppAPI.Application.Features.Orders.Commands.CreateOrder;
using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IBasketReadRepository : IReadRepository<Entities.Basket>
    {
        Task<ActiveBasketOrderData?> GetActiveBasketOrderDataAsync(string userId, CancellationToken cancellationToken);
        Task<Entities.Basket?> GetActiveBasketByUserIdAsync(string userId, CancellationToken cancellationToken);
        Task<List<BasketItemListDto>> GetItemsByBasketIdAsync(Guid basketId, CancellationToken cancellationToken);
    }
}
