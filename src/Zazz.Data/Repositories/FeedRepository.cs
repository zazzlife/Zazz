using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.EntityClient;
using System.Data.SqlClient;
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
            return (from feed in DbSet
                    from feedUserId in feed.FeedUserIds
                    orderby feed.Time descending 
                    where userIds.Contains(feedUserId.UserId)
                    select feed)
                .Distinct()
                .Include(f => f.FeedPhotoIds)
                .Include(f => f.Post)
                .Include(f => f.Event);
        }

        public IQueryable<Feed> GetUserFeeds(int userId)
        {
            return (from feed in DbSet
                    from feedUserId in feed.FeedUserIds
                    orderby feed.Time descending 
                    where feedUserId.UserId == userId
                    select feed)
                .Distinct()
                .Include(f => f.FeedPhotoIds)
                .Include(f => f.Post)
                .Include(f => f.Event);
        }

        public Feed GetUserLastFeed(int userId)
        {
            return (from feed in DbSet
                    from feedUserId in feed.FeedUserIds
                    orderby feed.Time descending
                    where feedUserId.UserId == userId
                    select feed)
                .Include(f => f.FeedPhotoIds)
                .FirstOrDefault();
        }

        public void RemoveEventFeeds(int eventId)
        {
            const string DELETE_COMMAND = "DELETE FROM dbo.Feeds WHERE EventId = @eventId";
            var parameter = new SqlParameter
                            {
                                ParameterName = "eventId",
                                Value = eventId
                            };

            DbContext.Database.ExecuteSqlCommand(DELETE_COMMAND, parameter);
        }

        public void RemovePostFeeds(int postId)
        {
            const string DELETE_COMMAND = "DELETE FROM dbo.Feeds WHERE PostId = @postId";
            var parameter = new SqlParameter
            {
                ParameterName = "postId",
                Value = postId
            };

            DbContext.Database.ExecuteSqlCommand(DELETE_COMMAND, parameter);
        }
    }
}