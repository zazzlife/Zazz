using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Xml.Linq;
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
            return DbSet.Where(f => userIds.Contains(f.UserId)).Include(f => f.FeedPhotoIds);
        }

        public IQueryable<Feed> GetUserFeeds(int userId)
        {
            return DbSet.Where(f => f.UserId == userId).Include(f => f.FeedPhotoIds);
        }

        public Feed GetUserLastFeed(int userId)
        {
            return DbSet.Where(f => f.UserId == userId)
                        .Include(f => f.FeedPhotoIds)
                        .OrderByDescending(f => f.Time)
                        .FirstOrDefault();
        }

        public void RemoveEventFeeds(int eventId)
        {
            var items = DbSet.Where(f => f.EventId == eventId);
            foreach (var item in items)
                Remove(item);
        }

        public void RemovePostFeeds(int postId)
        {
            var items = DbSet.Where(f => f.PostId == postId);
            foreach (var item in items)
                Remove(item);
        }
    }
}