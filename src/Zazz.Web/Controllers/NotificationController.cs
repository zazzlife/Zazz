using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        private readonly INotificationService _notificationService;

        public const byte NOTIFICATIONS_PAGE_SIZE = 30;

        public NotificationController(IUserService userService, IPhotoService photoService,
            INotificationService notificationService)
        {
            _userService = userService;
            _photoService = photoService;
            _notificationService = notificationService;
        }

        public ActionResult Index()
        {
            var currentUserId = _userService.GetUserId(User.Identity.Name);
            var notificationsVm = GetNotifications(currentUserId, NOTIFICATIONS_PAGE_SIZE);

            var vm = new NotificationsPageViewModel
                     {
                         CurrentUserDisplayName = User.Identity.Name,
                         CurrentUserPhoto = _photoService.GetUserImageUrl(currentUserId),
                         Notifications = notificationsVm
                     };

            return View(vm);
        }

        public ActionResult Get(long? lastNotification, int take = 5)
        {
            var userId = _userService.GetUserId(User.Identity.Name);
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
                    EventName = n.NotificationType == NotificationType.NewEvent
                                ? n.EventNotification.Event.Name
                                : String.Empty,
                    ItemId = 
                                n.NotificationType == NotificationType.WallPost 
                                ? n.PostNotification.PostId
                                
                                : n.NotificationType == NotificationType.NewEvent
                                ? n.EventNotification.EventId

                                : n.NotificationType == NotificationType.CommentOnPhoto
                                ? n.Comment.PhotoComment.PhotoId

                                : n.NotificationType == NotificationType.CommentOnPost
                                ? n.Comment.PostComment.PostId
                                
                                : n.NotificationType == NotificationType.CommentOnEvent 
                                ? n.Comment.EventComment.EventId

                                :0,
                                
                    NotificationType = n.NotificationType,
                    UserDisplayName = _userService.GetUserDisplayName(n.UserBId),
                    UserPhoto = _photoService.GetUserImageUrl(n.UserBId),
                    PhotoViewModel = n.NotificationType != NotificationType.CommentOnPhoto ? null
                    : new PhotoViewModel
                    {
                        FromUserDisplayName = _userService.GetUserDisplayName(n.Comment.PhotoComment.Photo.UserId),
                        FromUserId = n.Comment.PhotoComment.Photo.UserId,
                        FromUserPhotoUrl = _photoService.GetUserImageUrl(n.Comment.PhotoComment.Photo.UserId),
                        IsFromCurrentUser = userId == n.Comment.PhotoComment.Photo.UserId,
                        PhotoDescription = n.Comment.PhotoComment.Photo.Description,
                        PhotoId = n.Comment.PhotoComment.PhotoId,
                        PhotoUrl = n.Comment.PhotoComment.Photo.IsFacebookPhoto
                        ? new PhotoLinks(n.Comment.PhotoComment.Photo.FacebookLink)
                        : _photoService.GeneratePhotoUrl(n.Comment.PhotoComment.Photo.UserId, n.Comment.PhotoComment.PhotoId)
                    }
                });

            return notificationsVm;
        }

        public void MarkAll()
        {
            var userId = _userService.GetUserId(User.Identity.Name);
            _notificationService.MarkUserNotificationsAsRead(userId);
        }

        public void Remove(int id)
        {
            var userId = _userService.GetUserId(User.Identity.Name);
            _notificationService.Remove(id, userId);
        }

        public string GetNewNotificationsCount()
        {
            var userId = _userService.GetUserId(User.Identity.Name);
            var newNotificationsCount = _notificationService.GetUnreadNotificationsCount(userId);

            if (newNotificationsCount == 0)
                return String.Empty;
            else
            {
                return
                    "<span id=\"new-notifications-count\" class=\"badge badge-small badge-info\">"
                    + newNotificationsCount + "</span>";
            }
        }
    }
}
