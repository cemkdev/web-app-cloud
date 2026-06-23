using E = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IBasketReadRepository : IReadRepository<E.Basket>
    {
        Task<E.Basket?> GetWithItemsAndProductImagesAsync(Guid basketId, bool tracking = false);
    }
}
