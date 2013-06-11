using System;
using System.Linq;
using Zazz.Core.Interfaces;
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

        public ObjectMapper(IUserService userService, IPhotoService photoService,
            IDefaultImageHelper defaultImageHelper)
        {
            _userService = userService;
            _photoService = photoService;
            _defaultImageHelper = defaultImageHelper;
        }

        public User RegisterVmToUser(RegisterViewModel registerVm)
        {
            var user = new User
                       {
                           Email = registerVm.Email,
                           LastActivity = DateTime.UtcNow,
                           Username = registerVm.UserName,
                           AccountType = registerVm.AccountType,
                           JoinedDate = DateTime.UtcNow,
                           Preferences = new UserPreferences
                                         {
                                             SendSyncErrorNotifications = true,
                                             SyncFbEvents = true,
                                             SyncFbImages = registerVm.AccountType == AccountType.Club,
                                             SyncFbPosts = registerVm.AccountType == AccountType.Club
                                         }
                       };

            if (registerVm.AccountType == AccountType.Club)
            {
                user.ClubDetail = new ClubDetail
                                  {
                                      ClubName = registerVm.ClubName,
                                      Address = registerVm.ClubAddress,
                                      ClubType = registerVm.ClubType,
                                  };
            }
            else
            {
                user.UserDetail = new UserDetail
                                  {
                                      Gender = registerVm.Gender,
                                      PublicEmail = registerVm.PublicEmail,
                                      SchoolId = registerVm.SchoolId,
                                      FullName = registerVm.FullName,
                                      MajorId = registerVm.MajorId,
                                      CityId = registerVm.CityId,
                                  };
            }

            return user;
        }


        #region API

        public ApiPost PostToApiPost(Post post)
        {
            return new ApiPost
                   {
                       FromUserDisplayName = _userService.GetUserDisplayName(post.FromUserId),
                       FromUserDisplayPhoto = _photoService.GetUserImageUrl(post.FromUserId),
                       FromUserId = post.FromUserId,
                       Message = post.Message,
                       PostId = post.Id,
                       Time = post.CreatedTime,
                       ToUserId = post.ToUserId,
                       ToUserDisplayName = post.ToUserId.HasValue
                                               ? _userService.GetUserDisplayName(post.ToUserId.Value)
                                               : null,
                       ToUserDisplayPhoto = post.ToUserId.HasValue
                                                ? _photoService.GetUserImageUrl(post.ToUserId.Value)
                                                : null,
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
                       UserDisplayPhoto = _photoService.GetUserImageUrl(photo.UserId)
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
                       UserDisplayPhoto = _photoService.GetUserImageUrl(e.UserId)
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
                       PhotoLinks = weekly.PhotoId.HasValue
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
                UserDisplayPhoto = feed.UserImageUrl,
                CanCurrentUserRemoveFeed = feed.CurrentUserCanRemoveFeed || feed.IsFromCurrentUser,
                Time = feed.Time,

                Comments = feed.CommentsViewModel.Comments == null
                ? null //Enumerable.Empty<ApiComment>()
                : feed.CommentsViewModel.Comments.Select(c => new ApiComment
                {
                    CommentId = c.CommentId,
                    CommentText = c.CommentText,
                    IsFromCurrentUser = c.IsFromCurrentUser,
                    UserId = c.UserId,
                    UserDisplayName = c.UserDisplayName,
                    UserDisplayPhoto = c.UserPhotoUrl,
                    Time = c.Time
                }),

                Photos = feed.FeedType == FeedType.Photo
                ? feed.PhotoViewModel.Select(p => new ApiPhoto
                {
                    PhotoId = p.PhotoId,
                    AlbumId = p.AlbumId,
                    Description = p.PhotoDescription,
                    UserId = p.FromUserId,
                    UserDisplayName = p.FromUserDisplayName,
                    UserDisplayPhoto = p.FromUserPhotoUrl,
                    PhotoLinks = p.PhotoUrl
                })
                : null,

                Post = feed.FeedType == FeedType.Post
                ? new ApiPost
                {
                    Message = feed.PostViewModel.PostText,
                    PostId = feed.PostViewModel.PostId,
                    FromUserId = feed.UserId,
                    FromUserDisplayName = feed.UserDisplayName,
                    FromUserDisplayPhoto = feed.UserImageUrl,
                    ToUserDisplayName = feed.PostViewModel.ToUserDisplayName,
                    ToUserId = feed.PostViewModel.ToUserId,
                    ToUserDisplayPhoto = feed.PostViewModel.ToUserPhotoUrl,
                    Time = feed.Time
                }
                : null,

                ApiEvent = feed.FeedType == FeedType.Event
                ? new ApiEvent
                {
                    City = feed.EventViewModel.City,
                    CreatedTime = feed.EventViewModel.CreatedDate.HasValue
                    ? feed.EventViewModel.CreatedDate.Value
                    : DateTime.MinValue,
                    Description = feed.EventViewModel.Description,
                    EventId = feed.EventViewModel.Id,
                    FacebookLink = feed.EventViewModel.FacebookEventId.HasValue
                        ? "https://www.facebook.com/events/" + feed.EventViewModel.FacebookEventId.Value
                        : null,
                    ImageUrl = feed.EventViewModel.ImageUrl,
                    IsDateOnly = feed.EventViewModel.IsDateOnly,
                    IsFacebookEvent = feed.EventViewModel.IsFacebookEvent,
                    Latitude = feed.EventViewModel.Latitude,
                    Longitude = feed.EventViewModel.Longitude,
                    Location = feed.EventViewModel.Location,
                    Name = feed.EventViewModel.Name,
                    Price = feed.EventViewModel.Price,
                    Street = feed.EventViewModel.Street,
                    Time = feed.EventViewModel.Time,
                    UtcTime = feed.EventViewModel.Time.ToUniversalTime().DateTime,
                    UserDisplayName = feed.UserDisplayName,
                    UserDisplayPhoto = feed.UserImageUrl,
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
                UserDisplayPhoto = commentViewModel.UserPhotoUrl,
                UserId = commentViewModel.UserId
            };
        }

        #endregion
    }
}