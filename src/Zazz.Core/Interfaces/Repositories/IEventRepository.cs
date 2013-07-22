using System;
using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface IEventRepository : IRepository<ZazzEvent>
    {
        IQueryable<ZazzEvent> GetUserEvents(int userId, int take, int? lastEventId = null);

        int GetUpcomingEventsCount(int userId);

        int GetOwnerId(int eventId);

        IQueryable<ZazzEvent> GetEventRange(DateTime from, DateTime to, DateTime? from2, DateTime? to2);

        ZazzEvent GetByFacebookId(long fbEventId);

        void ResetPhotoId(int photoId);

        IEnumerable<int> GetPageEventIds(int pageId);
    }
}