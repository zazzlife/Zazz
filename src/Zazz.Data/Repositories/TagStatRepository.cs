using System;
using System.Data.Entity;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class TagStatRepository : BaseRepository<TagStat>, ITagStatRepository
    {
        public TagStatRepository(DbContext dbContext) : base(dbContext)
        {}

        protected override int GetItemId(TagStat item)
        {
            throw new InvalidOperationException("You must provide Id to update an entity, if you want to insert, user InsertGraph");
        }

        public TagStat GetLastestTagStat(byte tagId)
        {
            return DbSet.Include(t => t.TagUsers)
                        .Where(t => t.TagId == tagId)
                        .OrderByDescending(t => t.Date)
                        .FirstOrDefault();
        }

        public void IncrementUsersCount(int id)
        {
            throw new NotImplementedException();
        }

        public int GetUsersCount(int tagId)
        {
            throw new NotImplementedException();
        }
    }
}