using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DynamicTranslate.DB
{
    public class Repository<TEntity> where TEntity : class
    {
        private readonly DbContext _databaseContext;
        public readonly DbSet<TEntity> DbSet;

        public Repository(DbContext databaseContext/*, IServiceProvider serviceProvider*/)
        {
            _databaseContext = databaseContext;
            _databaseContext.ChangeTracker.LazyLoadingEnabled = false;
            DbSet = _databaseContext.Set<TEntity>();
        }

        public void ClearEntities()
        {
            List<EntityEntry> dataList = _databaseContext.ChangeTracker.Entries().ToList();
            Parallel.ForEach(dataList, entityEntry => entityEntry.State = EntityState.Detached);
        }
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return _databaseContext.SaveChangesAsync(cancellationToken);
        }
    }
}
