using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public abstract class BaseRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected ZazzDbContext DbContext { get; set; }

        protected IDbSet<T> DbSet { get; set; }

        protected BaseRepository(DbContext dbContext)
        {
            DbContext = dbContext as ZazzDbContext;
            if (DbContext == null)
                throw new InvalidCastException("Passed DbContext should be type of ZazzDbContext");

            DbSet = DbContext.Set<T>();
        }

        public IQueryable<T> GetAll()
        {
            return DbSet;
        }

        public virtual void InsertGraph(T item)
        {
            DbSet.Add(item);
        }

        public virtual void InsertOrUpdate(T item)
        {
            if (item.Id == default(int))
            {
                var itemId = GetItemId(item);
                if (itemId == default(int))
                {
                    DbContext.Entry(item).State = EntityState.Added;
                }
                else
                {
                    item.Id = itemId;
                    DbContext.Entry(item).State = EntityState.Modified;
                }
            }
            else
            {
                DbContext.Entry(item).State = EntityState.Modified;
            }
        }

        public virtual Task<T> GetByIdAsync(int id)
        {
            return Task.Run(() => DbSet.Find(id));
        }

        protected abstract int GetItemId(T item);

        public virtual async Task<bool> ExistsAsync(int id)
        {
            return await Task.Run(() => DbSet.Any(i => i.Id == id));
        }

        public virtual async Task RemoveAsync(int id)
        {
            if (id == default(int))
                throw new ArgumentException("Id was 0", "id");

            var item = await GetByIdAsync(id);
            if (item != null)
                DbSet.Remove(item);
        }

        public virtual void Remove(T item)
        {
            if (item == null || item.Id == default(int))
                throw new ArgumentException("item was not valid", "item");

            DbContext.Entry(item).State = EntityState.Deleted;
        }
    }
}