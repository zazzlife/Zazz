using System;
using System.Linq;
using System.Security;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
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

        public IQueryable<Notification> GetUserNotifications(int userId, long? lastNotificationId)
        {
            return _uow.NotificationRepository.GetUserNotifications(userId, lastNotificationId);
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
                                   UserId = fromUserId,
                                   UserBId = toUserId,
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
                                   CommentNotification = new CommentNotification { CommentId = commentId },
                                   UserBId = commenterId,
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
                                   CommentNotification = new CommentNotification { CommentId = commentId },
                                   UserBId = commenterId,
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
                                   CommentNotification = new CommentNotification { CommentId = commentId },
                                   UserBId = commenterId,
                                   Time = DateTime.UtcNow,
                                   IsRead = false,
                                   NotificationType = NotificationType.CommentOnEvent
                               };

            CreateNotification(notification, save);
        }

        public void CreateWallPostNotification(int fromUserId, int toUserId, int postId, bool save = true)
        {
            var notification = new Notification
                               {
                                   UserId = toUserId,
                                   UserBId = fromUserId,
                                   PostNotification = new PostNotification { PostId = postId },
                                   Time = DateTime.UtcNow,
                                   IsRead = false,
                                   NotificationType = NotificationType.WallPost
                               };

            CreateNotification(notification, save);
        }

        public void CreateTagPostNotification(int fromUserId, int toUserId, int postId, bool save = true)
        {
            var notification = new Notification
            {
                UserId = toUserId,
                UserBId = fromUserId,
                PostNotification = new PostNotification { PostId = postId },
                Time = DateTime.UtcNow,
                IsRead = false,
                NotificationType = NotificationType.TagNotification
            };

            CreateNotification(notification, save);
        }

        public void CreatelikePhotoNotification(int fromUserId, int toUserId, int photoId, bool save = true)
        {
            var notification = new Notification
            {
                UserId = toUserId,
                UserBId = fromUserId,
                Time = DateTime.UtcNow,
                IsRead = false,
                NotificationType = NotificationType.PhotoLike
            };

            CreateNotification(notification, save);
        }

        public void CreateLikePostNotification(int fromUserId, int toUserId, int postId, bool save = true)
        {
            
            var notification = new Notification
            {
                UserId = toUserId,
                UserBId = fromUserId,
                PostNotification = new PostNotification { PostId = postId },
                Time = DateTime.UtcNow,
                IsRead = false,
                NotificationType = NotificationType.PostLike
            };

            CreateNotification(notification, save);
            
        }

        public void CreateTagPhotoPostNotification(int fromUserId, int toUserId, int photoId, bool save = true)
        {
            var notification = new Notification
            {
                UserId = toUserId,
                UserBId = fromUserId,
                Time = DateTime.UtcNow,
                IsRead = false,
                NotificationType = NotificationType.TagPhotoNotification
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
                                       EventNotification = new EventNotification { EventId = eventId },
                                       Time = DateTime.UtcNow,
                                       NotificationType = NotificationType.NewEvent
                                   };

                _uow.NotificationRepository.InsertGraph(notification);
            }

            if (save)
                _uow.SaveChanges();
        }


        public void CreateNewEventInvitationNotification(int creatorUserId,int eventId,int[] toUserId,bool save = true)
        {
            foreach(int i in toUserId)
            {
                var notification = new Notification
                {
                    UserId = i,
                    UserBId = creatorUserId,
                    IsRead = false,
                    EventNotification = new EventNotification { EventId = eventId },
                    Time = DateTime.UtcNow,
                    NotificationType = NotificationType.EventInvitation
                };
                _uow.NotificationRepository.InsertGraph(notification);
            }

            if (save)
                _uow.SaveChanges();
        }

        public void RemoveFollowAcceptedNotification(int fromUserId, int toUserId, bool save = true)
        {
            _uow.NotificationRepository.RemoveFollowAcceptedNotification(fromUserId, toUserId);

            if (save)
                _uow.SaveChanges();
        }

        public void Remove(long notificationId, int currentUserId)
        {
            var notification = _uow.NotificationRepository.GetById(notificationId);
            if (notification == null)
                return;

            if (notification.UserId != currentUserId)
                throw new SecurityException();

            _uow.NotificationRepository.Remove(notification);
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