using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    [Authorize]
    public class NotificationsController : BaseController
    {
        private readonly INotificationService _notificationService;

// ReSharper disable MemberCanBePrivate.Global
        public const byte NOTIFICATIONS_PAGE_SIZE = 30;
// ReSharper restore MemberCanBePrivate.Global

        public NotificationsController(IUserService userService, IPhotoService photoService,
                                       INotificationService notificationService, IDefaultImageHelper defaultImageHelper,
                                       ICategoryService categoryService, IStaticDataRepository staticDataRepository)
            : base(userService, photoService, defaultImageHelper, staticDataRepository, categoryService)
        {
            _notificationService = notificationService;
        }

        public ActionResult Index()
        {
            var currentUserId = UserService.GetUserId(User.Identity.Name);
            var notificationsVm = GetNotifications(currentUserId, NOTIFICATIONS_PAGE_SIZE);

            var vm = new NotificationsPageViewModel
                     {
                         Notifications = notificationsVm
                     };

            return View(vm);
        }

        public ActionResult Get(long? lastNotification, int take = 5)
        {
            var userId = UserService.GetUserId(User.Identity.Name);
            var notifications = GetNotifications(userId, take, lastNotification);

            return View("_Notifications", notifications);
        }

        private IEnumerable<NotificationViewModel> GetNotifications(int userId, int take, long? lastNotificationId = null)
        {
            var notifications = _notificationService.GetUserNotifications(userId, lastNotificationId)
                .Take(take)
                .ToList();

            var notificationsVm = notifications
                .Select(n => new NotificationViewModel
                {
                    NotificationId = n.Id,
                    UserId = n.UserBId,
                    Time = n.Time,
                    IsRead = n.IsRead,
                    EventName = 
                                n.NotificationType == NotificationType.NewEvent
                                ? n.EventNotification.Event.Name
                                
                                : n.NotificationType == NotificationType.CommentOnEvent
                                ? n.CommentNotification.Comment.EventComment.Event.Name
                                
                                : String.Empty,
                    ItemId =
                                n.NotificationType == NotificationType.WallPost
                                ? n.PostNotification.PostId

                                : n.NotificationType == NotificationType.NewEvent
                                ? n.EventNotification.EventId

                                : n.NotificationType == NotificationType.CommentOnPhoto
                                ? n.CommentNotification.Comment.PhotoComment.PhotoId

                                : n.NotificationType == NotificationType.CommentOnPost
                                ? n.CommentNotification.Comment.PostComment.PostId

                                : n.NotificationType == NotificationType.CommentOnEvent
                                ? n.CommentNotification.Comment.EventComment.EventId

                                : 0,

                    NotificationType = n.NotificationType,
                    UserDisplayName = UserService.GetUserDisplayName(n.UserBId),
                    UserPhoto = PhotoService.GetUserDisplayPhoto(n.UserBId),
                    PhotoViewModel = n.NotificationType != NotificationType.CommentOnPhoto ? null
                    : new PhotoViewModel
                    {
                        FromUserDisplayName = UserService.GetUserDisplayName(n.CommentNotification.Comment.PhotoComment.Photo.UserId),

                        FromUserId = n.CommentNotification.Comment.PhotoComment.Photo.UserId,

                        FromUserPhotoUrl = PhotoService.GetUserDisplayPhoto(n.CommentNotification.Comment.PhotoComment.Photo.UserId),

                        IsFromCurrentUser = userId == n.CommentNotification.Comment.PhotoComment.Photo.UserId,

                        Description = n.CommentNotification.Comment.PhotoComment.Photo.Description,

                        PhotoId = n.CommentNotification.Comment.PhotoComment.PhotoId,

                        PhotoUrl = PhotoService.GeneratePhotoUrl(n.CommentNotification.Comment.PhotoComment.Photo.UserId, n.CommentNotification.Comment.PhotoComment.PhotoId)
                    }
                });

            return notificationsVm;
        }

        public void MarkAll()
        {
            var userId = UserService.GetUserId(User.Identity.Name);
            _notificationService.MarkUserNotificationsAsRead(userId);
        }

        public void Remove(int id)
        {
            var userId = UserService.GetUserId(User.Identity.Name);
            _notificationService.Remove(id, userId);
        }

        public string GetNewNotificationsCount()
        {
            var userId = UserService.GetUserId(User.Identity.Name);
            var newNotificationsCount = _notificationService.GetUnreadNotificationsCount(userId);

            if (newNotificationsCount == 0)
                return String.Empty;
            else
            {
                return
                    "<span style=\"margin-left: 3px;\" id=\"new-notifications-count\" class=\"badge badge-small badge-info\">"
                    + newNotificationsCount + "</span>";
            }
        }
    }
}
