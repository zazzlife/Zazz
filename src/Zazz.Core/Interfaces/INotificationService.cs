using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface INotificationService
    {
        IQueryable<Notification> GetUserNotifications(int userId);

        void CreateNotification(Notification notification, bool save = true);

        void CreateFollowApprovedNotification(int fromUserId, int toUserId, bool save = true);

        void CreatePhotoCommentNotification(int photoId, int photoOwnerUserId, bool save = true);

        void CreatePostCommentNotification(int postId, int postOwnerUserId, bool save = true);

        void CreateEventCommentNotification(int eventId, int eventOwnerUserId, bool save = true);

        void CreateWallPostNotification(int fromUserId, int toUserId, bool save = true);

        void CreateNewEventNotification(int creatorUserId, bool save = true);

        void RemovePhotoNotifications(int photoId);
        
        void RemovePostNotifications(int postId);
        
        void RemoveEventNotifications(int eventId);

        void MarkUserNotificationsAsRead(int userId);

        int GetUnreadNotificationsCount(int userId);
    }
}