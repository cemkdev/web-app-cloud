using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Persistence.Contexts;
using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Repositories
{
    public class EndpointReadRepository(WebAppAPIDbContext context)
        : ReadRepository<Entities.Endpoint>(context), IEndpointReadRepository
    {
        public Task<List<Entities.Endpoint>> GetAllWithMenuAndRolesAsync(CancellationToken cancellationToken, bool tracking = false)
            => Query(tracking)
                .Include(endpoint => endpoint.Menu)
                .Include(endpoint => endpoint.Roles)
                .ToListAsync(cancellationToken);

        public Task<List<Entities.Endpoint>> GetAuthorizedByRoleIdsAsync(IReadOnlyCollection<string> roleIds, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(roleIds);

            return Query(tracking: false)
                    .Where(endpoint =>
                        endpoint.Roles.Any(role => roleIds.Contains(role.Id)))
                    .Include(endpoint => endpoint.Menu)
                    .Include(endpoint => endpoint.Roles
                        .Where(role => roleIds.Contains(role.Id)))
                    .ToListAsync(cancellationToken);
        }

        public Task<bool> HasAnyUserRoleForEndpointAsync(string code, IReadOnlyCollection<string> userRoleNames, CancellationToken cancellationToken)
            => Query(tracking: false)
                .AnyAsync(endpoint =>
                    endpoint.Code == code &&
                    endpoint.Roles.Any(role =>
                        role.Name != null &&
                        userRoleNames.Contains(role.Name)),
                    cancellationToken);

        public Task<bool?> IsAdminOnlyByCodeAsync(string code, CancellationToken cancellationToken)
             => Query(tracking: false)
                    .Where(endpoint => endpoint.Code == code)
                    .Select(endpoint => (bool?)endpoint.AdminOnly)
                    .FirstOrDefaultAsync(cancellationToken);
    }
}
