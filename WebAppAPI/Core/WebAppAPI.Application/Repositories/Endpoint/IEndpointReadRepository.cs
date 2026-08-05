using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IEndpointReadRepository : IReadRepository<Entities.Endpoint>
    {
        Task<List<Entities.Endpoint>> GetAllWithMenuAndRolesAsync(CancellationToken cancellationToken, bool tracking = false);
        Task<List<Entities.Endpoint>> GetAuthorizedByRoleIdsAsync(IReadOnlyCollection<string> roleIds, CancellationToken cancellationToken);
        Task<bool> HasAnyUserRoleForEndpointAsync(string code, IReadOnlyCollection<string> roleNames, CancellationToken cancellationToken);
        Task<bool?> IsAdminOnlyByCodeAsync(string code, CancellationToken cancellationToken);
    }
}
