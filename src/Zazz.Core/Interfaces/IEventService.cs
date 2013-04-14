using System;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IEventService
    {
        /// <summary>
        /// Creates an event
        /// </summary>
        /// <param name="zazzEvent"></param>
        /// <returns></returns>
        void CreateEvent(ZazzEvent zazzEvent);

        /// <summary>
        /// Updates an event.
        /// </summary>
        /// <param name="zazzEvent">The event to update. (Id is required)</param>
        /// <param name="currentUserId">UserId of the current user. (Used for security check)</param>
        /// <returns></returns>
        void UpdateEvent(ZazzEvent zazzEvent, int currentUserId);

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