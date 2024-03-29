﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Filters;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [OAuth2Authorize]
    public class NotificationsController : BaseApiController
    {
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        private readonly IObjectMapper _objectMapper;

        public NotificationsController(INotificationService notificationService,
            IUserService userService, IPhotoService photoService, IObjectMapper objectMapper)
        {
            _notificationService = notificationService;
            _userService = userService;
            _photoService = photoService;
            _objectMapper = objectMapper;
        }

        // GET api/v1/notifications
        public IEnumerable<ApiNotification> Get(long? lastNotification = null)
        {
            var userId = CurrentUserId;
            var notifications = _notificationService.GetUserNotifications(userId, lastNotification)
                                                    .Take(30);
            
            List<ApiNotification> list = new List<ApiNotification>();
            foreach(Notification n in notifications.ToList())
            {
                list.Add(new ApiNotification{
                    NotificationId = n.Id,
                    IsRead = n.IsRead,
                    NotificationType = n.NotificationType,
                    Time = n.Time,
                    UserId = n.UserBId,
                    DisplayName = _userService.GetUserDisplayName(n.UserBId),
                    DisplayPhoto = _photoService.GetUserDisplayPhoto(n.UserBId),

                    // PHOTO
                    Photo = n.NotificationType == NotificationType.CommentOnPhoto
                        ? _objectMapper.PhotoToApiPhoto(n.CommentNotification.Comment.PhotoComment.Photo)
                        : null,

                    // POST
                    Post = n.NotificationType == NotificationType.WallPost
                        ? _objectMapper.PostToApiPost(n.PostNotification.Post)
                        : n.NotificationType == NotificationType.CommentOnPost
                            ? _objectMapper.PostToApiPost(
                            n.CommentNotification.Comment.PostComment.Post)
                            : null,

                    // EVENT
                    Event = (n.NotificationType == NotificationType.NewEvent || n.NotificationType == NotificationType.EventInvitation)
                        ? _objectMapper.EventToApiEvent(n.EventNotification.Event)
                        : n.NotificationType == NotificationType.CommentOnEvent
                            ? _objectMapper.EventToApiEvent
                            (n.CommentNotification.Comment.EventComment.Event)
                            : null
                });
            }

            return list;
        }

        // DELETE api/v1/notifications/5
        public void Delete(long id)
        {
            if (id < 1)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            try
            {
                var userId = CurrentUserId;
                _notificationService.Remove(id, userId);
            }
            catch (SecurityException)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
        }
    }
}
