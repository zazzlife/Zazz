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
using Zazz.Infrastructure;
using Zazz.Infrastructure.Helpers;
using Zazz.Web.Filters;
using Zazz.Web.Helpers;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [OAuth2Authorize]
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


        public IEnumerable<ApiUser> GetAllUsers()
        {
            List<ApiUser> list = new List<ApiUser>();

            var users = _userService.getAllUsers();

            foreach (var user in users)
            {
                var response = new ApiUser
                {
                    AccountType = user.AccountType,
                    Id = user.Id,
                    Username = user.Username,
                    ProfilePhotoId = user.ProfilePhotoId
                };
                list.Add(response);
            }


            IEnumerable<ApiUser> vm = list;

            return vm;
        }

        public IEnumerable<ApiUser> GetAllClubs()
        {
            List<ApiUser> list = new List<ApiUser>();

            var users = _userService.getAllClubs();

            foreach (var user in users)
            {
                var response = new ApiUser
                {
                    AccountType = user.AccountType,
                    Id = user.Id,
                    Username = user.Username,
                    ProfilePhotoId = user.ProfilePhotoId
                };
                list.Add(response);
            }


            IEnumerable<ApiUser> vm = list;

            return vm;
        }

        // GET /api/v1/user
        public ApiUser Get()
        {
            var userId = CurrentUserId;
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
                response.ProfilePhoto = user.ProfilePhotoId.HasValue
                                            ? _photoService.GeneratePhotoUrl(user.Id, user.ProfilePhotoId.Value)
                                            : _defaultImageHelper.GetUserDefaultImage(user.UserDetail.Gender);

                response.UserDetails = new ApiUserDetails
                                       {
                                           City = user.UserDetail.City,
                                           FullName = user.UserDetail.FullName,
                                           Gender = user.UserDetail.Gender,
                                           Major = user.UserDetail.Major,
                                           School = user.UserDetail.School,
                                       };
            }
            else
            {
                response.ProfilePhoto = user.ProfilePhotoId.HasValue
                                            ? _photoService.GeneratePhotoUrl(user.Id, user.ProfilePhotoId.Value)
                                            : _defaultImageHelper.GetUserDefaultImage(Gender.NotSpecified);

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

                response.Preferences.SyncFbPhotos = user.Preferences.SyncFbImages;
                response.Preferences.SyncFbPosts = user.Preferences.SyncFbPosts;
            }

            return response;
        }

        // PUT /api/v1/user
        public void Put([FromBody] ApiUser u)
        {
            var userId = CurrentUserId;
            var user = _uow.UserRepository.GetById(userId, true, true, false, true);

            if (u.Preferences == null ||
                (user.AccountType == AccountType.User && u.UserDetails == null) ||
                (user.AccountType == AccountType.Club && u.ClubDetails == null))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            user.ProfilePhotoId = u.ProfilePhotoId;

            user.Preferences.SendSyncErrorNotifications = u.Preferences.SendSyncErrorNotifications;
            user.Preferences.SyncFbEvents = u.Preferences.SyncFbEvents;
            
            if (user.AccountType == AccountType.User)
            {
                user.UserDetail.CityId = u.UserDetails.CityId;
                user.UserDetail.Gender = u.UserDetails.Gender;
                user.UserDetail.FullName = u.UserDetails.FullName;
                user.UserDetail.MajorId = u.UserDetails.MajorId;
                user.UserDetail.SchoolId = u.UserDetails.SchoolId;
            }
            else
            {
                user.ClubDetail.Address = u.ClubDetails.Address;
                user.ClubDetail.ClubName = u.ClubDetails.ClubName;
                user.ClubDetail.ClubType = u.ClubDetails.ClubType;
                user.ClubDetail.CoverPhotoId = u.ClubDetails.CoverPhotoId;

                if (u.Preferences.SyncFbPhotos.HasValue)
                    user.Preferences.SyncFbImages = u.Preferences.SyncFbPhotos.Value;

                if (u.Preferences.SyncFbPosts.HasValue)
                    user.Preferences.SyncFbPosts = u.Preferences.SyncFbPosts.Value;
            }

            _uow.SaveChanges();
        }
    }
}
