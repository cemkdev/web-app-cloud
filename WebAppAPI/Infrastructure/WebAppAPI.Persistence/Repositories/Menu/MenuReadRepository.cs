using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Domain.Entities;
using WebAppAPI.Persistence.Contexts;

namespace WebAppAPI.Persistence.Repositories
{
    public class MenuReadRepository(WebAppAPIDbContext context)
        : ReadRepository<Menu>(context), IMenuReadRepository
    {
        public Task<List<Menu>> GetAllMenusAsync(CancellationToken cancellationToken, bool tracking = false)
            => Query(tracking)
                .ToListAsync(cancellationToken);
    }
}
