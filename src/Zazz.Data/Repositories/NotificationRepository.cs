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
                .Include(n => n.EventNotification)
                .Include(n => n.EventNotification.Event)
                .Include(n => n.PostNotification)
                .Include(n => n.UserB)
                .Include(n => n.Comment)
                .Include(n => n.Comment.PhotoComment)
                .Include(n => n.Comment.PostComment)
                .Include(n => n.Comment.EventComment)
                .Include(n => n.Comment.PhotoComment.Photo)
                .Include(n => n.Comment.PostComment.Post)
                .Include(n => n.Comment.EventComment.Event)
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

        public void RemoveRecordsByCommentId(int commentId)
        {
            var notifications = DbSet.Where(n => n.CommentId == commentId).ToList();
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