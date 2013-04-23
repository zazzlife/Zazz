using System.Data.Entity;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class NotificationRepository : BaseLongRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(DbContext dbContext) : base(dbContext)
        {}

        public void RemoveRecordsByPhotoId(int photoId)
        {
            var notifications = DbSet.Where(n => n.PhotoId == photoId);
            foreach (var notification in notifications)
            {
                Remove(notification);
            }
        }

        public void RemoveRecordsByPostId(int postId)
        {
            var notifications = DbSet.Where(n => n.PostId == postId);
            foreach (var notification in notifications)
            {
                Remove(notification);
            }
                
        }

        public void RemoveRecordsByEventId(int eventId)
        {
            var notifications = DbSet.Where(n => n.EventId == eventId);
            foreach (var notification in notifications)
            {
                Remove(notification);
            }   
        }

        public void MarkUserNotificationsAsRead(int userId)
        {
            var notifications = DbSet.Where(n => n.UserId == userId && !n.IsRead);
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }
        }

        public int GetUnreadNotificationsCount(int userId)
        {
            throw new System.NotImplementedException();
        }
    }
}