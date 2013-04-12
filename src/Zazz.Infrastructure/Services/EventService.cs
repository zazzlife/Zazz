using System;
using System.Security;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class EventService : IEventService
    {
        private readonly IUoW _uow;

        public EventService(IUoW uow)
        {
            _uow = uow;
        }

        public void CreateEvent(ZazzEvent zazzEvent)
        {
            if (zazzEvent.UserId == 0)
                throw new ArgumentException("User id cannot be 0");

            zazzEvent.CreatedDate = DateTime.UtcNow;
            _uow.EventRepository.InsertGraph(zazzEvent);

            _uow.SaveChanges();

            var feed = new Feed
                       {
                           FeedType = FeedType.Event,
                           EventId = zazzEvent.Id,
                           UserId = zazzEvent.UserId,
                           Time = zazzEvent.CreatedDate
                       };

            _uow.FeedRepository.InsertGraph(feed);
            _uow.SaveChanges();
        }

        public async Task UpdateEventAsync(ZazzEvent zazzEvent, int currentUserId)
        {
            if (zazzEvent.Id == 0)
                throw new ArgumentException();

            var currentOwner = await _uow.EventRepository.GetOwnerIdAsync(zazzEvent.Id);
            if (currentOwner != currentUserId)
                throw new SecurityException();

            // if you want to set update datetime later, the place would be here!
            _uow.EventRepository.InsertOrUpdate(zazzEvent);
            _uow.SaveChanges();
        }

        public async Task<ZazzEvent> GetEventAsync(int id)
        {
            return _uow.EventRepository.GetById(id);
        }

        public async Task DeleteEventAsync(int eventId, int currentUserId)
        {
            if (eventId == 0)
                throw new ArgumentException();

            var ownerId = await _uow.EventRepository.GetOwnerIdAsync(eventId);
            if (ownerId != currentUserId)
                throw new SecurityException();

            _uow.FeedRepository.RemoveEventFeeds(eventId);
            _uow.CommentRepository.RemoveEventComments(eventId);

            _uow.EventRepository.Remove(eventId);
            _uow.SaveChanges();
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}