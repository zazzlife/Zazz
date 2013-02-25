using System;
using System.Data.Entity;
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

        public void InsertGraph(T item)
        {
            throw new System.NotImplementedException();
        }

        public void InsertOrUpdate(T item)
        {
            throw new System.NotImplementedException();
        }

        public Task<T> GetByIdAsync(int id)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> ExistsAsync(int id)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(T item)
        {
            throw new System.NotImplementedException();
        }
    }
}