using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface INotificationRepository : ILongRepository<Notification>
    {
        IQueryable<Notification> GetUserNotifications(int userId, long? lastNotificationId);

        void RemoveFollowAcceptedNotification(int userId, int userBId);

        void RemoveRecordsByCommentId(int commentId);

        void MarkUserNotificationsAsRead(int userId);

        int GetUnreadNotificationsCount(int userId);
    }
}