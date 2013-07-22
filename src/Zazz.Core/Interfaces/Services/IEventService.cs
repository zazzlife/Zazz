using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Services
{
    public interface IEventService
    {
        /// <summary>
        /// Returns events that has been created by the user (in desc order)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="take">Number of rows to take</param>
        /// <param name="lastEventId"></param>
        /// <returns></returns>
        IQueryable<ZazzEvent> GetUserEvents(int userId, int take, int? lastEventId = null);

        /// <summary>
        /// Creates an event
        /// </summary>
        /// <param name="zazzEvent"></param>
        /// <returns></returns>
        void CreateEvent(ZazzEvent zazzEvent);

        /// <summary>
        /// Updates an event.
        /// </summary>
        /// <param name="updatedEvent">The event to update. (Id is required)</param>
        /// <param name="currentUserId">UserId of the current user. (Used for security check)</param>
        /// <returns></returns>
        void UpdateEvent(ZazzEvent updatedEvent, int currentUserId);

        /// <summary>
        /// Gets an entry
        /// </summary>
        /// <param name="id">Event id</param>
        /// <returns></returns>
        ZazzEvent GetEvent(int id);

        /// <summary>
        /// Deletes an event
        /// </summary>
        /// <param name="eventId">Id of the event to delete.</param>
        /// <param name="currentUserId">UserId of the current user. (Used for security check)</param>
        /// <returns></returns>
        void DeleteEvent(int eventId, int currentUserId);
    }
}