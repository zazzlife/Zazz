using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface IFeedRepository : IRepository<Feed>
    {
        IQueryable<Feed> GetFeedsWithCategories(List<byte> categories);

        IQueryable<Feed> GetFeeds(IEnumerable<int> userIds);

        IQueryable<Feed> GetUserFeeds(int userId);

        Feed GetPhotoFeed(int photoId);

        Feed GetPostFeed(int postId);

        Feed GetUserLastFeed(int userId);
    }
}