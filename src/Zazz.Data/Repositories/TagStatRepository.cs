using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class TagStatRepository : BaseRepository<TagStat>, ITagStatRepository
    {
        private static readonly object LockToken = new object();

        public TagStatRepository(DbContext dbContext) : base(dbContext)
        {}

        protected override int GetItemId(TagStat item)
        {
            throw new InvalidOperationException("You must provide Id to update an entity, if you want to insert, user InsertGraph");
        }

        public TagStat GetTagStat(byte tagId)
        {
            return DbSet.SingleOrDefault(t => t.TagId == tagId);
        }

        public int GetUsersCount(int tagId)
        {
            return DbSet
                .Where(t => t.TagId == tagId)
                .Select(t => t.UsersCount)
                .FirstOrDefault();
        }
    }
}