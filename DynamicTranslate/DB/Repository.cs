using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicTranslate.DB
{
    public class Repository<TEntity> where TEntity : class
    {
        private DbContext _databaseContext;
        public DbSet<TEntity> DbSet;

        public Repository(DbContext databaseContext, IServiceProvider serviceProvider)
        {
            _databaseContext = databaseContext;
            _databaseContext.ChangeTracker.LazyLoadingEnabled = false;
            DbSet = _databaseContext.Set<TEntity>();

        }

        public void ClearEntities()
        {
            try
            {
                _databaseContext.ChangeTracker.Entries().ToList()
                    .ForEach(x => x.State = EntityState.Detached);
            }
            catch (Exception ex)
            {
            }

        }
        public async Task SaveChangesAsync()
        {
            await _databaseContext.SaveChangesAsync();
        }
    }
}
