using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IMenuReadRepository : IReadRepository<Entities.Menu>
    {
        Task<List<Entities.Menu>> GetAllMenusAsync(CancellationToken cancellationToken, bool tracking = false);
    }
}
