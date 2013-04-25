﻿using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface INotificationService
    {
        IQueryable<Notification> GetUserNotifications(int userId);

        void CreateNotification(Notification notification, bool save = true);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromUserId">Id of the user that has requested the follow.</param>
        /// <param name="toUserId">Id of the user that has accepted the follow.</param>
        /// <param name="save"></param>
        void CreateFollowAcceptedNotification(int fromUserId, int toUserId, bool save = true);

        void CreatePhotoCommentNotification(int commentId, int photoId, int photoOwnerUserId, bool save = true);

        void CreatePostCommentNotification(int commentId, int postId, int postOwnerUserId, bool save = true);

        void CreateEventCommentNotification(int commentId, int eventId, int eventOwnerUserId, bool save = true);

        void CreateWallPostNotification(int fromUserId, int toUserId, bool save = true);

        void CreateNewEventNotification(int creatorUserId, int eventId, bool save = true);

        void RemovePhotoNotifications(int photoId);
        
        void RemovePostNotifications(int postId);
        
        void RemoveEventNotifications(int eventId);

        void RemoveCommentNotifications(int commentId);

        void MarkUserNotificationsAsRead(int userId);

        int GetUnreadNotificationsCount(int userId);
    }
}