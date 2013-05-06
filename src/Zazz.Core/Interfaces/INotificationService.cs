using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface INotificationService
    {
        IQueryable<Notification> GetUserNotifications(int userId, long? lastNotificationId);

        void CreateNotification(Notification notification, bool save = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromUserId">Id of the user that has requested the follow.</param>
        /// <param name="toUserId">Id of the user that has accepted the follow.</param>
        /// <param name="save"></param>
        void CreateFollowAcceptedNotification(int fromUserId, int toUserId, bool save = true);

        void CreatePhotoCommentNotification(int commentId, int commenterId, int photoId, int userToBeNotified, bool save = true);

        void CreatePostCommentNotification(int commentId, int commenterId, int postId, int userToBeNotified, bool save = true);

        void CreateEventCommentNotification(int commentId, int commenterId, int eventId, int userToBeNotified, bool save = true);

        void CreateWallPostNotification(int fromUserId, int toUserId, int postId, bool save = true);

        void CreateNewEventNotification(int creatorUserId, int eventId, bool save = true);

        void RemoveFollowAcceptedNotification(int fromUserId, int toUserId, bool save = true);
        
        void RemoveEventNotifications(int eventId);

        void RemoveCommentNotifications(int commentId);

        void Remove(long notificationId, int currentUserId);

        void MarkUserNotificationsAsRead(int userId);

        int GetUnreadNotificationsCount(int userId);
    }
}