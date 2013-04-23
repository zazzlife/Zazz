using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface INotificationService
    {
        IQueryable<Notification> GetUserNotifications(int userId);

        void CreateNotification(Notification notification, bool save = true);

        void RemovePhotoNotifications(int photoId);
        
        void RemovePostNotifications(int postId);
        
        void RemoveEventNotifications(int eventId);

        void MarkUserNotificationsAsRead(int userId);

        int GetUnreadNotificationsCount(int userId);
    }
}