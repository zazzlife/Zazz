using System;
using System.Data.Entity;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public abstract class BaseLongRepository<T> : IRepository<T> where T : BaseEntityLong
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
            throw new System.NotImplementedException();
        }

        public void InsertGraph(T item)
        {
            throw new System.NotImplementedException();
        }

        public void InsertOrUpdate(T item)
        {
            throw new System.NotImplementedException();
        }

        public T GetById(int id)
        {
            throw new System.NotImplementedException();
        }

        public bool Exists(int id)
        {
            throw new System.NotImplementedException();
        }

        public void Remove(int id)
        {
            throw new System.NotImplementedException();
        }

        public void Remove(T item)
        {
            throw new System.NotImplementedException();
        }
    }
}