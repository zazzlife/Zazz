using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class EventRepository : BaseRepository<ZazzEvent>, IEventRepository
    {
        public EventRepository(DbContext dbContext)
            : base(dbContext)
        {
        }

        public override void Remove(int id)
        {
            var zazzEvent = GetById(id);
            if (zazzEvent == null)
                return;

            DbContext.Entry(zazzEvent).State = EntityState.Deleted;
        }

        public override ZazzEvent GetById(int id)
        {
            return DbSet.SingleOrDefault(e => e.Id == id);
        }

        public IQueryable<ZazzEvent> GetUserEvents(int userId, int take, int? lastEventId = null)
        {
            var query = DbSet.Where(e => e.UserId == userId);
            
            if (lastEventId.HasValue)
                query = query.Where(e => e.Id < lastEventId.Value);

            return query
                .OrderByDescending(e => e.CreatedDate)
                .Take(take);
        }

        public int GetUpcomingEventsCount(int userId)
        {
            var today = DateTime.UtcNow.Date;

            return DbSet
                .Where(e => e.UserId == userId)
                .Where(e => EntityFunctions.TruncateTime(e.TimeUtc) >= today)
                .Count();
        }

        public int GetOwnerId(int eventId)
        {
            return DbSet.Where(e => e.Id == eventId).Select(e => e.UserId).SingleOrDefault();
        }

        public IQueryable<ZazzEvent> GetEventRange(DateTime @from, DateTime to, DateTime? from2, DateTime? to2)
        {

            var query = DbSet.AsQueryable();
            

            if (!from2.HasValue || !to2.HasValue)
            {
                query = DbSet.Where(e => EntityFunctions.TruncateTime(e.TimeUtc) >= from)
                             .Where(e => EntityFunctions.TruncateTime(e.TimeUtc) <= to);
            }
            else
            {
                query = query.Where(e =>
                                    (EntityFunctions.TruncateTime(e.TimeUtc) >= from &&
                                     EntityFunctions.TruncateTime(e.TimeUtc) <= to) ||
                                    (EntityFunctions.TruncateTime(e.TimeUtc) >= from2 &&
                                     EntityFunctions.TruncateTime(e.TimeUtc) <= to2));
            }
            

            return query;
        }

        public ZazzEvent GetByFacebookId(long fbEventId)
        {
            return DbSet.SingleOrDefault(e => e.FacebookEventId == fbEventId);
        }

        public void ResetPhotoId(int photoId)
        {
            var events = DbSet.Where(e => e.PhotoId == photoId).ToList();
            foreach (var e in events)
            {
                e.PhotoId = null;
            }
        }

        public IEnumerable<int> GetPageEventIds(int pageId)
        {
            return DbSet.Where(e => e.PageId == pageId)
                        .Select(e => e.Id);
        }
    }
}