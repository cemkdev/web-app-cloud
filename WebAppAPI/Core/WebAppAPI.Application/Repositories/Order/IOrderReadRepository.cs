using E = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IOrderReadRepository : IReadRepository<E.Order>
    {
        Task<(List<E.Order> Orders, int TotalCount)> GetPagedWithBasketSummaryAsync(int page, int size);
        Task<E.Order?> GetDetailByIdAsync(Guid id, bool tracking = false);
        Task<E.Order?> GetWithBasketUserAsync(Guid id, bool tracking = false);
        Task<bool> HasOrderForBasketAsync(Guid basketId);
    }
}
