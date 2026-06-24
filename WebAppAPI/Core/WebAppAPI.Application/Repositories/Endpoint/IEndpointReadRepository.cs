using E = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IEndpointReadRepository : IReadRepository<E.Endpoint>
    {
        Task<List<E.Endpoint>> GetAllWithMenuAndRolesAsync(bool tracking = true);
        Task<E.Endpoint?> GetByCodeWithMenuAsync(string code, bool tracking = false);
        Task<bool?> GetAdminOnlyByCodeAsync(string code);
    }
}
