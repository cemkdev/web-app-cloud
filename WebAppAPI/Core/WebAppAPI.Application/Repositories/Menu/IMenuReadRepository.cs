using E = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IMenuReadRepository : IReadRepository<E.Menu>
    {
        Task<List<E.Menu>> GetAllMenusAsync(bool tracking = true);
    }
}
