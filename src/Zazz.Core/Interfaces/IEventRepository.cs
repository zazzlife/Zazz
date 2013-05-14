using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IEventRepository : IRepository<ZazzEvent>
    {
        int GetUpcomingEventsCount(int userId);

        int GetOwnerId(int eventId);

        IQueryable<ZazzEvent> GetEventRange(DateTime from, DateTime to, DateTime? from2, DateTime? to2);

        ZazzEvent GetByFacebookId(long fbEventId);

        void ResetPhotoId(int photoId);

        IEnumerable<int> GetPageEventIds(int pageId);
    }
}