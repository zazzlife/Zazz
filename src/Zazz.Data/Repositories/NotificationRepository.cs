using System.Data.Entity;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Data.Repositories
{
    public class NotificationRepository : BaseLongRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(DbContext dbContext) : base(dbContext)
        {}

        public IQueryable<Notification> GetUserNotifications(int userId, long? lastNotificationId)
        {
            var query = DbSet
                .Include(n => n.Event)
                .Include(n => n.Photo)
                .Include(n => n.Post)
                .Include(n => n.UserB)
                .Include(n => n.Comment)
                .Where(n => n.UserId == userId);

            if (lastNotificationId.HasValue)
                query = query.Where(n => n.Id < lastNotificationId);

            return query.OrderByDescending(n => n.Time);
        }

        public void RemoveFollowAcceptedNotification(int userId, int userBId)
        {
            var notifications = DbSet
                .Where(n => n.NotificationType == NotificationType.FollowRequestAccepted)
                .Where(n => n.UserId == userId)
                .Where(n => n.UserBId == userBId);

            foreach (var notification in notifications)
            {
                Remove(notification);
            }
        }

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

        public void RemoveRecordsByCommentId(int commentId)
        {
            var notifications = DbSet.Where(n => n.CommentId == commentId);
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
            return DbSet.Count(n => n.UserId == userId && !n.IsRead);
        }
    }
}