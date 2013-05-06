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
            //TODO: this query can be optimized, it's reading a lot of data that won't be used.
            var query = DbSet
                .Include(n => n.EventNotification)
                .Include(n => n.EventNotification.Event)
                .Include(n => n.PostNotification)
                .Include(n => n.UserB)
                .Include(n => n.CommentNotification)
                .Include(n => n.CommentNotification.Comment.PhotoComment)
                .Include(n => n.CommentNotification.Comment.PostComment)
                .Include(n => n.CommentNotification.Comment.EventComment)
                .Include(n => n.CommentNotification.Comment.PhotoComment.Photo)
                .Include(n => n.CommentNotification.Comment.PostComment.Post)
                .Include(n => n.CommentNotification.Comment.EventComment.Event)
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
                .Where(n => n.UserBId == userBId)
                .ToList();

            foreach (var notification in notifications)
            {
                Remove(notification);
            }
        }

        public void MarkUserNotificationsAsRead(int userId)
        {
            var notifications = DbSet.Where(n => n.UserId == userId && !n.IsRead).ToList();
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