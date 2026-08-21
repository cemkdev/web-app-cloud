using System.Linq.Expressions;
using WebAppAPI.Domain.Entities.Common;

namespace WebAppAPI.Application.Repositories
{
    public interface IWriteRepository<T> where T : BaseEntity
    {
        Task<bool> AddAsync(T model);
        Task<bool> AddRangeAsync(List<T> data);

        bool Update(T model);

        bool Remove(T model);
        bool RemoveRange(List<T> data);
        Task<bool> RemoveAsync(Guid id);

        Task<int> ExecuteDeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken);
    }
}
