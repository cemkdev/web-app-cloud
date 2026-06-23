using WebAppAPI.Domain.Entities.Common;

namespace WebAppAPI.Application.Repositories
{
    public interface IWriteRepository<T> where T : BaseEntity
    {
        Task<bool> AddAsync(T model);
        Task<bool> AddRangeAsync(List<T> data);
        bool Remove(T model);
        bool RemoveRange(List<T> data);
        Task<bool> RemoveAsync(Guid id);
        bool Update(T model);
    }
}
