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

        public IQueryable<Feed> GetFeedsWithTags(List<byte> tags)
        {
            var query = (from feed in DbSet
                         from photoTag in feed.FeedPhotos.SelectMany(p => p.Photo.Tags).DefaultIfEmpty()
                         from postTag in feed.PostFeed.Post.Tags.DefaultIfEmpty()
                         from eventTag in feed.EventFeed.Event.Tags.DefaultIfEmpty()
                         orderby feed.Time descending
                         where
                             tags.Contains(photoTag.CategoryId) ||
                             tags.Contains(postTag.CategoryId) ||
                             tags.Contains(eventTag.CategoryId)
                         select feed)
                .Distinct()
                .Include(f => f.FeedPhotos)
                .Include(f => f.PostFeed.Post)
                .Include(f => f.EventFeed.Event);


            return query;
        }

        public IQueryable<Feed> GetFeeds(IEnumerable<int> userIds)
        {
            return (from feed in DbSet
                    from feedUserId in feed.FeedUsers
                    where userIds.Contains(feedUserId.UserId)
                    select feed)
                .Distinct()
                .Include(f => f.FeedPhotos)
                .Include(f => f.PostFeed.Post)
                .Include(f => f.EventFeed.Event);
        }

        public IQueryable<Feed> GetUserFeeds(int userId)
        {
            return (from feed in DbSet
                    from feedUserId in feed.FeedUsers
                    where feedUserId.UserId == userId
                    select feed)
                .Distinct()
                .Include(f => f.FeedPhotos)
                .Include(f => f.PostFeed.Post)
                .Include(f => f.EventFeed.Event);
        }

        public Feed GetPostFeed(int postId)
        {
            return DbSet
                .Include(f => f.PostFeed.Post)
                .FirstOrDefault(f => f.PostFeed.PostId == postId);
        }

        public Feed GetUserLastFeed(int userId)
        {
            return (from feed in DbSet
                    from feedUserId in feed.FeedUsers
                    orderby feed.Time descending
                    where feedUserId.UserId == userId
                    select feed)
                .Include(f => f.FeedPhotos)
                .FirstOrDefault();
        }
    }
}