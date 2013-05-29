using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.Web
{
    public partial class ObjectMapper : IObjectMapper
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
                       PhotoId = photo.Id,
                       PhotoLinks = photo.IsFacebookPhoto
                                        ? new PhotoLinks(photo.FacebookLink)
                                        : _photoService.GeneratePhotoUrl(photo.UserId, photo.Id),
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
                       Id = weekly.Id,
                       Name = weekly.Name,
                       UserId = weekly.UserId,
                       PhotoLinks = weekly.PhotoId.HasValue
                                        ? _photoService.GeneratePhotoUrl(weekly.UserId, weekly.PhotoId.Value)
                                        : _defaultImageHelper.GetDefaultWeeklyImage()
                   };
        }
    }
}