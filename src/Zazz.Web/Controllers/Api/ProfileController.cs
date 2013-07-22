using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Filters;
using Zazz.Web.Helpers;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [OAuth2Authorize]
    public class ProfileController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        private readonly IFollowService _followService;
        private readonly IDefaultImageHelper _defaultImageHelper;
        private readonly IFeedHelper _feedHelper;
        private readonly IObjectMapper _objectMapper;

        public ProfileController(IUserService userService, IPhotoService photoService,
            IFollowService followService, IDefaultImageHelper defaultImageHelper, IFeedHelper feedHelper,
            IObjectMapper objectMapper)
        {
            _userService = userService;
            _photoService = photoService;
            _followService = followService;
            _defaultImageHelper = defaultImageHelper;
            _feedHelper = feedHelper;
            _objectMapper = objectMapper;
        }

        // GET api/v1/users/5/profile
        public ApiUserProfile Get(int id, int lastFeed = 0)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var currentUserId = CurrentUserId;

            var user = _userService.GetUser(id, true, true, true);
            if (user == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var isSelf = currentUserId == id;
            bool? followRequestSent = null;
            bool? isCurrentUserFollowingTargetUser = null;
            bool? isTargetUserFollowingCurrentUser = null;

            if (!isSelf)
            {
                if (user.AccountType == AccountType.User)
                    followRequestSent = _followService.IsFollowRequestExists(currentUserId, id);

                isCurrentUserFollowingTargetUser = _followService.IsFollowing(currentUserId, id);
                isTargetUserFollowingCurrentUser = _followService.IsFollowing(id, currentUserId);
            }

            var userDisplayName = _userService.GetUserDisplayName(id);
            var userDisplayPhoto = _photoService.GetUserImageUrl(id);

            return new ApiUserProfile
                   {
                       AccountType = user.AccountType,
                       DisplayName = userDisplayName,
                       DisplayPhoto = userDisplayPhoto,
                       FollowRequestAlreadySent = followRequestSent,
                       FollowersCount = _followService.GetFollowersCount(id),
                       Id = user.Id,
                       IsSelf = isSelf,
                       IsCurrentUserFollowingTargetUser = isCurrentUserFollowingTargetUser,
                       IsTargetUserFollowingCurrentUser = isTargetUserFollowingCurrentUser,
                       Feeds = _feedHelper.GetUserActivityFeed(id, currentUserId, lastFeed)
                       .Select(_objectMapper.FeedViewModelToApiModel),
                       Photos = _photoService.GetLatestUserPhotos(id, 15)
                       .ToList().Select(_objectMapper.PhotoToApiPhoto),
                       UserDetails = user.AccountType == AccountType.User
                       ? new ApiUserDetails
                         {
                             City = user.UserDetail.City,
                             FullName = user.UserDetail.FullName,
                             Gender = user.UserDetail.Gender,
                             Major = user.UserDetail.Major,
                             School = user.UserDetail.School,
                         }
                      : null,
                       ClubDetails = user.AccountType == AccountType.Club
                      ? new ApiClubDetails
                         {
                             Address = user.ClubDetail.Address,
                             ClubName = user.ClubDetail.ClubName,
                             ClubType = user.ClubDetail.ClubType,
                             CoverPhoto = user.ClubDetail.CoverPhotoId.HasValue
                             ? _photoService.GeneratePhotoUrl(id, user.ClubDetail.CoverPhotoId.Value)
                             : _defaultImageHelper.GetDefaultCoverImage()
                         }
                      : null,
                       Weeklies = user.AccountType == AccountType.Club
                       ? user.Weeklies.Select(_objectMapper.WeeklyToApiWeekly)
                     : null
                   };
        }
    }
}
