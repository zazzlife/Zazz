using System;
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

        protected override int GetItemId(ZazzEvent item)
        {
            throw new InvalidOperationException("You should always provide the id for updating the event, if it's new then use insert graph.");
        }

        public override async Task RemoveAsync(int id)
        {
            var zazzEvent = await GetByIdAsync(id);
            if (zazzEvent == null)
                return;

            DbContext.Entry(zazzEvent).State = EntityState.Deleted;
        }

        public Task<int> GetOwnerIdAsync(int eventId)
        {
            return Task.Run(() => DbSet.Where(e => e.Id == eventId).Select(e => e.UserId).SingleOrDefault());
        }

        public IQueryable<ZazzEvent> GetEventRange(DateTime from, DateTime to)
        {
            return DbSet.Where(p => EntityFunctions.TruncateTime(p.TimeUtc) >= from)
                        .Where(p => EntityFunctions.TruncateTime(p.TimeUtc) <= to);
        }

        public ZazzEvent GetByFacebookId(long fbEventId)
        {
            return DbSet.SingleOrDefault(e => e.FacebookEventId == fbEventId);
        }

        public void ResetPhotoId(int photoId)
        {
            var events = DbSet.Where(p => p.PhotoId == photoId);
            foreach (var e in events)
            {
                e.PhotoId = null;
            }
        }
    }
}