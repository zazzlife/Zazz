using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure;
using Zazz.Infrastructure.Helpers;
using Zazz.Web.Filters;
using Zazz.Web.Helpers;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class UserController : BaseApiController
    {
        private readonly IUoW _uow;
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        private readonly IDefaultImageHelper _defaultImageHelper;

        public UserController(IUoW uow, IUserService userService, IPhotoService photoService,
            IDefaultImageHelper defaultImageHelper)
        {
            _uow = uow;
            _userService = userService;
            _photoService = photoService;
            _defaultImageHelper = defaultImageHelper;
        }

        // GET /api/v1/user
        public ApiUser Get()
        {
            var userId = ExtractUserIdFromHeader();
            var user = _userService.GetUser(userId, true, true, false, true);

            var response = new ApiUser
                           {
                               AccountType = user.AccountType,
                               Id = user.Id,
                               Username = user.Username,
                               ProfilePhotoId = user.ProfilePhotoId,
                               Preferences = new ApiUserPreferences
                                             {
                                                 SendSyncErrorNotifications = user.Preferences.SendSyncErrorNotifications,
                                                 SyncFbEvents = user.Preferences.SyncFbEvents
                                             }
                           };

            if (user.AccountType == AccountType.User)
            {
                response.UserDetails = new ApiUserDetails
                                       {
                                           City = user.UserDetail.City,
                                           FullName = user.UserDetail.FullName,
                                           Gender = user.UserDetail.Gender,
                                           Major = user.UserDetail.Major,
                                           School = user.UserDetail.School
                                       };
            }
            else
            {
                response.ClubDetails = new ApiClubDetails
                                       {
                                           Address = user.ClubDetail.Address,
                                           ClubName = user.ClubDetail.ClubName,
                                           ClubType = user.ClubDetail.ClubType,
                                           CoverPhotoId = user.ClubDetail.CoverPhotoId,
                                           CoverPhoto = user.ClubDetail.CoverPhotoId.HasValue
                                               ? _photoService
                                                 .GeneratePhotoUrl(user.Id, user.ClubDetail.CoverPhotoId.Value)
                                               : _defaultImageHelper.GetDefaultCoverImage()
                                       };

                response.Preferences.SyncFbImages = user.Preferences.SyncFbImages;
                response.Preferences.SyncFbPosts = user.Preferences.SyncFbPosts;
            }

            return response;
        }

        // PUT /api/v1/user
        public void Put(ApiUser user)
        {
            throw new NotImplementedException();
        }
    }
}
