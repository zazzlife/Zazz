using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IEventRepository : IRepository<ZazzEvent>
    {
        Task<int> GetOwnerIdAsync(int eventId);

        IQueryable<ZazzEvent> GetEventRange(DateTime from, DateTime to);

        ZazzEvent GetByFacebookId(long fbEventId);

        void ResetPhotoId(int photoId);

        IEnumerable<int> GetPageEventIds(int pageId);
    }
}