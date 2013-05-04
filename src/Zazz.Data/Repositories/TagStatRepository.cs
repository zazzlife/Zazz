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

        public TagStat GetLastestTagStat(byte tagId)
        {
            return DbSet.Include(t => t.TagUsers)
                        .Where(t => t.TagId == tagId)
                        .OrderByDescending(t => t.Date)
                        .FirstOrDefault();
        }

        public void UpdateUsersCount(int id)
        {
            lock (LockToken) //NOTE: if you move the app to a web farm this lock won't help much.
            {
                var record = DbSet
                    .Include(t => t.TagUsers)
                    .FirstOrDefault(t => t.Id == id);

                if (record == null)
                    return;

                record.UsersCount = record.TagUsers.Count;
            }
        }

        public int GetUsersCount(int tagId)
        {
            throw new NotImplementedException();
        }
    }
}