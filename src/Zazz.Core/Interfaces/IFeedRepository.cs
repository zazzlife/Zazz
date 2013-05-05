using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IFeedRepository : IRepository<Feed>
    {
        IQueryable<Feed> GetFeeds(IEnumerable<int> userIds);

        IQueryable<Feed> GetUserFeeds(int userId);

        Feed GetPostFeed(int postId);

        Feed GetUserLastFeed(int userId);
    }
}