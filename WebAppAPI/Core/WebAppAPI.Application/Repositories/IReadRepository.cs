using WebAppAPI.Domain.Entities.Common;

namespace WebAppAPI.Application.Repositories
{
    public interface IReadRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken, bool tracking = false);

        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
    }
}
