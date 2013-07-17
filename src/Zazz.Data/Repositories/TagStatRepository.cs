using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class TagStatRepository : BaseRepository<CategoryStat>, ITagStatRepository
    {
        public TagStatRepository(DbContext dbContext) : base(dbContext)
        {}

        public override IQueryable<CategoryStat> GetAll()
        {
            return DbSet.Include(t => t.Category);
        }

        public CategoryStat GetTagStat(byte tagId)
        {
            return DbSet.SingleOrDefault(t => t.CategoryId == tagId);
        }

        public int GetUsersCount(int tagId)
        {
            return DbSet
                .Where(t => t.CategoryId == tagId)
                .Select(t => t.UsersCount)
                .FirstOrDefault();
        }
    }
}