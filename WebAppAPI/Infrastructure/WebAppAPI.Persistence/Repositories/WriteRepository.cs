using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Domain.Entities.Common;
using WebAppAPI.Persistence.Contexts;

namespace WebAppAPI.Persistence.Repositories
{
    public class WriteRepository<T>(WebAppAPIDbContext context) : IWriteRepository<T> where T : BaseEntity
    {
        protected DbSet<T> Set => context.Set<T>();

        public async Task<bool> AddAsync(T model)
        {
            EntityEntry<T> entityEntry = await Set.AddAsync(model);
            return entityEntry.State == EntityState.Added;
        }

        public async Task<bool> AddRangeAsync(List<T> data)
        {
            await Set.AddRangeAsync(data);
            return true;
        }

        public bool Update(T model)
        {
            EntityEntry<T> entityEntry = Set.Update(model);
            return entityEntry.State == EntityState.Modified;
        }

        public bool Remove(T model)
        {
            EntityEntry<T> entityEntry = Set.Remove(model);
            return entityEntry.State == EntityState.Deleted;
        }

        public bool RemoveRange(List<T> data)
        {
            Set.RemoveRange(data);
            return true;
        }

        public async Task<bool> RemoveAsync(Guid id)
        {
            T? model = await Set.FirstOrDefaultAsync(data => data.Id == id);
            if (model is null)
                return false;

            return Remove(model);
        }

        public Task<int> ExecuteDeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
            => Set
                .Where(predicate)
                .ExecuteDeleteAsync(cancellationToken);
    }
}
