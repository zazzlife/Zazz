using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Web.Filters;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [OAuth2Authorize]
    public class MeController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;

        public MeController(IUserService userService, IPhotoService photoService)
        {
            _userService = userService;
            _photoService = photoService;
        }

        public ApiBasicUserInfo Get()
        {
            var user = _userService.GetUser(CurrentUserId);

            return new ApiBasicUserInfo
                   {
                       AccountType = user.AccountType,
                       DisplayName = _userService.GetUserDisplayName(user.Id),
                       DisplayPhoto = _photoService.GetUserPhoto(user.Id),
                       IsConfirmed = user.IsConfirmed,
                       UserId = user.Id
                   };
        }

    }


}
