using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Persistence.Contexts;
using E = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Repositories
{
    public class MenuReadRepository(WebAppAPIDbContext context)
        : ReadRepository<E.Menu>(context), IMenuReadRepository
    {
        public Task<List<E.Menu>> GetAllMenusAsync(bool tracking = true)
            => Query(tracking).ToListAsync();
    }
}
