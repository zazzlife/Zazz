using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public abstract class BaseLongRepository<T> : ILongRepository<T> where T : BaseEntityLong
    {
        protected DbContext DbContext { get; set; }

        protected IDbSet<T> DbSet { get; set; }

        protected BaseLongRepository(DbContext dbContext)
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

        public void InsertGraph(T item)
        {
            DbSet.Add(item);
        }

        public void InsertOrUpdate(T item)
        {
            if (item.Id == 0)
            {
                throw new InvalidOperationException("You need to provide the id for updating the record, if you want to insert use InsertGraph instead");
            }

            DbContext.Entry(item).State = EntityState.Modified;

        }

        public T GetById(long id)
        {
            return DbSet.Find(id);
        }

        public bool Exists(long id)
        {
            return DbSet.Any(x => x.Id == id);
        }

        public void Remove(long id)
        {
            var item = GetById(id);
            if (item != null)
            {
                Remove(item);
            }
        }

        public void Remove(T item)
        {
            DbContext.Entry(item).State = EntityState.Deleted;
        }
    }
}