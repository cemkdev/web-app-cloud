using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Persistence.Contexts;
using E = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Repositories
{
    public class EndpointReadRepository(WebAppAPIDbContext context)
        : ReadRepository<E.Endpoint>(context), IEndpointReadRepository
    {
        public Task<List<E.Endpoint>> GetAllWithMenuAndRolesAsync(bool tracking = true)
            => Query(tracking)
                .Include(e => e.Menu)
                .Include(e => e.Roles)
                .ToListAsync();

        public Task<E.Endpoint?> GetByCodeWithMenuAsync(string code, bool tracking = false)
            => Query(tracking)
                .Include(e => e.Menu)
                .Include(e => e.Roles)
                .FirstOrDefaultAsync(e => e.Code == code);

        public async Task<bool?> GetAdminOnlyByCodeAsync(string code)
        {
            return await Query(false)
                .Where(e => e.Code == code)
                .Select(e => (bool?)e.AdminOnly)
                .FirstOrDefaultAsync();
        }
    }
}
