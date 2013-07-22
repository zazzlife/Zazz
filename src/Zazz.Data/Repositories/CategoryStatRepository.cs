using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class CategoryStatRepository : BaseRepository<CategoryStat>, ICategoryStatRepository
    {
        public CategoryStatRepository(DbContext dbContext) : base(dbContext)
        {}

        public override IQueryable<CategoryStat> GetAll()
        {
            return DbSet.Include(t => t.Category);
        }

        public CategoryStat GetById(byte categoryId)
        {
            return DbSet.SingleOrDefault(t => t.CategoryId == categoryId);
        }

        public int GetUsersCount(int categoryId)
        {
            return DbSet
                .Where(t => t.CategoryId == categoryId)
                .Select(t => t.UsersCount)
                .FirstOrDefault();
        }
    }
}