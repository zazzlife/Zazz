using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class FeedRepository : BaseRepository<Feed>, IFeedRepository
    {
        public FeedRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(Feed item)
        {
            throw new InvalidOperationException("You need to provide the id for updating this record, Use InsertGraph for inserting new record");
        }

        public IQueryable<Feed> GetFeeds(IEnumerable<int> userIds)
        {
            return DbSet.Where(f => userIds.Contains(f.UserId));
        }
    }
}