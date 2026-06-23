using Microsoft.EntityFrameworkCore;
using E = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IEndpointReadRepository : IReadRepository<E.Endpoint>
    {
        DbSet<E.Endpoint> Table { get; }

        Task<List<E.Endpoint>> GetAllWithMenuAndRolesAsync(bool tracking = true);
        Task<E.Endpoint?> GetByCodeWithMenuAsync(string code, bool tracking = false);
    }
}
