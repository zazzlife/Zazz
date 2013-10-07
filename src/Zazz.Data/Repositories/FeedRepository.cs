using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Xml.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class FeedRepository : BaseRepository<Feed>, IFeedRepository
    {
        public FeedRepository(DbContext dbContext) : base(dbContext)
        {
        }

        public IQueryable<Feed> GetFeedsWithCategories(List<byte> categories)
        {
            var query = (from feed in DbSet
                         from photoCategory in feed.FeedPhotos.SelectMany(p => p.Photo.Categories).DefaultIfEmpty()
                         from postCategory in feed.PostFeed.Post.Categories.DefaultIfEmpty()
                         orderby feed.Time descending
                         where
                             categories.Contains(photoCategory.CategoryId) ||
                             categories.Contains(postCategory.CategoryId)
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
                .Include(f => f.PostFeed.Post.Categories)
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
                .Include(f => f.PostFeed.Post.Categories)
                .Include(f => f.EventFeed.Event);
        }

        public Feed GetPhotoFeed(int photoId)
        {
            return DbSet.Include(f => f.FeedPhotos)
                        .Where(f => f.FeedPhotos.Any(p => p.PhotoId == photoId))
                        .SingleOrDefault();
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