using System;
using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface IFeedRepository : IRepository<Feed>
    {
        IQueryable<Feed> GetFeedsWithCategories(IEnumerable<int> userIds, IEnumerable<byte> categories);

        IQueryable<Feed> GetFeedsWithCategoriesTags(IEnumerable<int> userIds, IEnumerable<byte> categories, IEnumerable<int> tags);

        IQueryable<Feed> GetFeedsWithTags(IEnumerable<int> userIds, IEnumerable<int> tags);

        IQueryable<Feed> GetFeeds(IEnumerable<int> userIds);

        IQueryable<Feed> GetUserFeeds(int userId);

        IQueryable<Feed> GetUserLikedFeeds(int userId);

        DateTime GetFeedDateTime(int feedId);

        Feed GetPhotoFeed(int photoId);

        Feed GetPostFeed(int postId);

        Feed GetUserLastFeed(int userId);
    }
}