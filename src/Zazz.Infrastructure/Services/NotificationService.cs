using System;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUoW _uow;

        public NotificationService(IUoW uow)
        {
            _uow = uow;
        }

        public IQueryable<Notification> GetUserNotifications(int userId)
        {
            return _uow.NotificationRepository.GetUserNotifications(userId);
        }

        public void CreateNotification(Notification notification, bool save = true)
        {
            _uow.NotificationRepository.InsertGraph(notification);

            if (save)
                _uow.SaveChanges();
        }

        public void CreateFollowAcceptedNotification(int fromUserId, int toUserId, bool save = true)
        {
            var notification = new Notification
                               {
                                   UserId = toUserId,
                                   UserBId = fromUserId,
                                   Time = DateTime.UtcNow,
                                   IsRead = false,
                                   NotificationType = NotificationType.FollowRequestAccepted
                               };

            CreateNotification(notification, save);
        }

        public void CreatePhotoCommentNotification(int commentId, int commenterId, int photoId, int userToBeNotified, bool save = true)
        {
            var notification = new Notification
                               {
                                   UserId = userToBeNotified,
                                   CommentId = commentId,
                                   UserBId = commentId,
                                   PhotoId = photoId,
                                   Time = DateTime.UtcNow,
                                   IsRead = false,
                                   NotificationType = NotificationType.CommentOnPhoto
                               };

            CreateNotification(notification, save);
        }

        public void CreatePostCommentNotification(int commentId, int commenterId, int postId, int userToBeNotified, bool save = true)
        {
            var notification = new Notification
                               {
                                   UserId = userToBeNotified,
                                   CommentId = commentId,
                                   UserBId = commentId,
                                   PostId = postId,
                                   Time = DateTime.UtcNow,
                                   IsRead = false,
                                   NotificationType = NotificationType.CommentOnPost
                               };

            CreateNotification(notification, save);
        }

        public void CreateEventCommentNotification(int commentId, int commenterId, int eventId, int userToBeNotified, bool save = true)
        {
            var notification = new Notification
                               {
                                   UserId = userToBeNotified,
                                   CommentId = commentId,
                                   UserBId = commentId,
                                   EventId = eventId,
                                   Time = DateTime.UtcNow,
                                   IsRead = false,
                                   NotificationType = NotificationType.CommentOnEvent
                               };

            CreateNotification(notification, save);
        }

        public void CreateWallPostNotification(int fromUserId, int toUserId, bool save = true)
        {
            var notification = new Notification
                               {
                                   UserId = toUserId,
                                   UserBId = fromUserId,
                                   Time = DateTime.UtcNow,
                                   IsRead = false,
                                   NotificationType = NotificationType.WallPost
                               };

            CreateNotification(notification, save);
        }

        public void CreateNewEventNotification(int creatorUserId, int eventId, bool save = true)
        {
            var followers = _uow.FollowRepository.GetUserFollowers(creatorUserId)
                .Select(f => f.FromUserId);

            foreach (var followerId in followers)
            {
                var notification = new Notification
                                   {
                                       UserId = followerId,
                                       UserBId = creatorUserId,
                                       IsRead = false,
                                       EventId = eventId,
                                       Time = DateTime.UtcNow,
                                       NotificationType = NotificationType.NewEvent
                                   };

                _uow.NotificationRepository.InsertGraph(notification);
            }

            if (save)
                _uow.SaveChanges();
        }

        public void RemovePhotoNotifications(int photoId)
        {
            _uow.NotificationRepository.RemoveRecordsByPhotoId(photoId);
            _uow.SaveChanges();
        }

        public void RemovePostNotifications(int postId)
        {
            _uow.NotificationRepository.RemoveRecordsByPostId(postId);
            _uow.SaveChanges();
        }

        public void RemoveEventNotifications(int eventId)
        {
            _uow.NotificationRepository.RemoveRecordsByEventId(eventId);
            _uow.SaveChanges();
        }

        public void RemoveCommentNotifications(int commentId)
        {
            _uow.NotificationRepository.RemoveRecordsByCommentId(commentId);
            _uow.SaveChanges();
        }

        public void MarkUserNotificationsAsRead(int userId)
        {
            _uow.NotificationRepository.MarkUserNotificationsAsRead(userId);
            _uow.SaveChanges();
        }

        public int GetUnreadNotificationsCount(int userId)
        {
            return _uow.NotificationRepository.GetUnreadNotificationsCount(userId);
        }
    }
}