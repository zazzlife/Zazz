using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IFeedRepository : IRepository<Feed>
    {
        IQueryable<Feed> GetFeeds(IEnumerable<int> userIds);

        IQueryable<Feed> GetUserFeeds(int userId);

        void RemovePhotoFeed(int photoId);

        void RemoveEventFeed(int eventId);
    }
}