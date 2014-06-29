using System;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Interfaces;
using Zazz.Web.Models;
using Zazz.Web.Models.Api;

namespace Zazz.Web
{
    public class ObjectMapper : IObjectMapper
    {
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        private readonly IDefaultImageHelper _defaultImageHelper;
        private readonly IFeedHelper _feedHelper;

        public ObjectMapper(IUserService userService, IPhotoService photoService,
            IDefaultImageHelper defaultImageHelper, IFeedHelper feedHelper)
        {
            _userService = userService;
            _photoService = photoService;
            _defaultImageHelper = defaultImageHelper;
            _feedHelper = feedHelper;
        }

        #region API

        public ApiPost PostToApiPost(Post post)
        {
            return new ApiPost
                   {
                       FromUserDisplayName = _userService.GetUserDisplayName(post.FromUserId),
                       FromUserDisplayPhoto = _photoService.GetUserDisplayPhoto(post.FromUserId),
                       FromUserId = post.FromUserId,
                       Message = _feedHelper.GetPostMsgItems(post.Message),
                       PostId = post.Id,
                       Time = post.CreatedTime,
                       ToUserId = post.ToUserId,
                       ToUserDisplayName = post.ToUserId.HasValue
                                               ? _userService.GetUserDisplayName(post.ToUserId.Value)
                                               : null,
                       ToUserDisplayPhoto = post.ToUserId.HasValue
                                                ? _photoService.GetUserDisplayPhoto(post.ToUserId.Value)
                                                : null,

                       Categories = post.Categories.Select(c => (int)c.CategoryId)
                   };
        }

        public ApiPhoto PhotoToApiPhoto(Photo photo)
        {
            return new ApiPhoto
                   {
                       Description = photo.Description,
                       AlbumId = photo.AlbumId,
                       PhotoId = photo.Id,
                       PhotoLinks = _photoService.GeneratePhotoUrl(photo.UserId, photo.Id),
                       UserId = photo.UserId,
                       UserDisplayName = _userService.GetUserDisplayName(photo.UserId),
                       UserDisplayPhoto = _photoService.GetUserDisplayPhoto(photo.UserId),
                       Categories = photo.Categories.Select(c => (int)c.CategoryId)
                   };
        }

        public ApiEvent EventToApiEvent(ZazzEvent e)
        {
            return new ApiEvent
                   {
                       City = e.City,
                       CreatedTime = e.CreatedDate,
                       Description = e.Description,
                       Name = e.Name,
                       UserId = e.UserId,
                       EventId = e.Id,
                       FacebookLink = e.IsFacebookEvent
                                          ? "https://www.facebook.com/events/" + e.FacebookEventId
                                          : null,
                       IsFacebookEvent = e.IsFacebookEvent,
                       IsDateOnly = e.IsDateOnly,
                       ImageUrl = e.IsFacebookEvent
                                      ? new PhotoLinks(e.FacebookPhotoLink)
                                      : e.PhotoId.HasValue
                                            ? _photoService.GeneratePhotoUrl(e.UserId, e.PhotoId.Value)
                                            : _defaultImageHelper.GetDefaultEventImage(),
                       Latitude = e.Latitude,
                       Location = e.Location,
                       Longitude = e.Longitude,
                       Price = e.Price,
                       Street = e.Street,
                       Time = e.Time,
                       UtcTime = e.Time.UtcDateTime,
                       UserDisplayName = _userService.GetUserDisplayName(e.UserId),
                       UserDisplayPhoto = _photoService.GetUserDisplayPhoto(e.UserId)
                   };
        }

        public ApiWeekly WeeklyToApiWeekly(Weekly weekly)
        {
            return new ApiWeekly
                   {
                       DayOfTheWeek = weekly.DayOfTheWeek,
                       Description = weekly.Description,
                       WeeklyId = weekly.Id,
                       Name = weekly.Name,
                       UserId = weekly.UserId,
                       Photo = weekly.PhotoId.HasValue
                                        ? _photoService.GeneratePhotoUrl(weekly.UserId, weekly.PhotoId.Value)
                                        : _defaultImageHelper.GetDefaultWeeklyImage()
                   };
        }

        public ApiFeed FeedViewModelToApiModel(FeedViewModel feed)
        {
            return new ApiFeed
            {
                FeedId = feed.FeedId,
                FeedType = feed.FeedType,
                UserId = feed.UserId,
                UserDisplayName = feed.UserDisplayName,
                UserDisplayPhoto = feed.UserDisplayPhoto,
                CanCurrentUserRemoveFeed = feed.CanCurrentUserRemoveFeed || feed.IsFromCurrentUser,
                Time = feed.Time,

                Comments = feed.Comments.Comments == null
                ? null //Enumerable.Empty<ApiComment>()
                : feed.Comments.Comments.Select(c => new ApiComment
                {
                    CommentId = c.CommentId,
                    CommentText = c.CommentText,
                    IsFromCurrentUser = c.IsFromCurrentUser,
                    UserId = c.UserId,
                    UserDisplayName = c.UserDisplayName,
                    UserDisplayPhoto = c.UserDisplayPhoto,
                    Time = c.Time
                }),

                Photos = feed.FeedType == FeedType.Photo
                ? feed.Photos.Select(p => new ApiPhoto
                {
                    PhotoId = p.PhotoId,
                    AlbumId = p.AlbumId,
                    Description = p.Description,
                    UserId = p.FromUserId,
                    UserDisplayName = p.FromUserDisplayName,
                    UserDisplayPhoto = p.FromUserPhotoUrl,
                    PhotoLinks = p.PhotoUrl
                })
                : null,

                Post = feed.FeedType == FeedType.Post
                ? new ApiPost
                {
                    Message = feed.Post.Message,
                    PostId = feed.Post.PostId,
                    FromUserId = feed.UserId,
                    FromUserDisplayName = feed.UserDisplayName,
                    FromUserDisplayPhoto = feed.UserDisplayPhoto,
                    ToUserDisplayName = feed.Post.ToUserDisplayName,
                    ToUserId = feed.Post.ToUserId,
                    ToUserDisplayPhoto = feed.Post.ToUserDisplayPhoto,
                    Time = feed.Time
                }
                : null,

                ApiEvent = feed.FeedType == FeedType.Event
                ? new ApiEvent
                {
                    City = feed.Event.City,
                    CreatedTime = feed.Event.CreatedDate.HasValue
                    ? feed.Event.CreatedDate.Value
                    : DateTime.MinValue,
                    Description = feed.Event.Description,
                    EventId = feed.Event.Id,
                    FacebookLink = feed.Event.FacebookEventId.HasValue
                        ? "https://www.facebook.com/events/" + feed.Event.FacebookEventId.Value
                        : null,
                    ImageUrl = feed.Event.ImageUrl,
                    IsDateOnly = feed.Event.IsDateOnly,
                    IsFacebookEvent = feed.Event.IsFacebookEvent,
                    Latitude = feed.Event.Latitude,
                    Longitude = feed.Event.Longitude,
                    Location = feed.Event.Location,
                    Name = feed.Event.Name,
                    Price = feed.Event.Price,
                    Street = feed.Event.Street,
                    Time = feed.Event.Time,
                    UtcTime = feed.Event.Time.ToUniversalTime().DateTime,
                    UserDisplayName = feed.UserDisplayName,
                    UserDisplayPhoto = feed.UserDisplayPhoto,
                    UserId = feed.UserId
                }
                : null
            };
        }

        public ApiComment CommentViewModelToApiModel(CommentViewModel commentViewModel)
        {
            return new ApiComment
            {
                CommentId = commentViewModel.CommentId,
                CommentText = commentViewModel.CommentText,
                IsFromCurrentUser = commentViewModel.IsFromCurrentUser,
                Time = commentViewModel.Time,
                UserDisplayName = commentViewModel.UserDisplayName,
                UserDisplayPhoto = commentViewModel.UserDisplayPhoto,
                UserId = commentViewModel.UserId
            };
        }

        #endregion
    }
}