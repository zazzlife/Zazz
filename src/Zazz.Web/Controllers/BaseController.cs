using System;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IUserService UserService;
        protected readonly IPhotoService PhotoService;
        protected readonly IDefaultImageHelper _defaultImageHelper;

        public BaseController(IUserService userService, IPhotoService photoService,
            IDefaultImageHelper defaultImageHelper)
        {
            UserService = userService;
            PhotoService = photoService;
            _defaultImageHelper = defaultImageHelper;
        }

        public string GetCurrentUserDisplayName()
        {
            if (!User.Identity.IsAuthenticated)
                return "Not Logged in!";

            return UserService.GetUserDisplayName(User.Identity.Name);
        }

        public string GetDisplayName(int id)
        {
            return UserService.GetUserDisplayName(id);
        }

        public string GetVerySmallDisplayPicture()
        {
            return GetDisplayPicture().VerySmallLink;
        }

        public PhotoLinks GetDisplayPicture()
        {
            if (!User.Identity.IsAuthenticated)
                return _defaultImageHelper.GetUserDefaultImage(Gender.NotSpecified);

            var userId = UserService.GetUserId(User.Identity.Name);
            return GetDisplayPicture(userId);
        }

        private PhotoLinks GetDisplayPicture(int id)
        {
            return PhotoService.GetUserImageUrl(id);
        }

        public void ShowAlert(string message, AlertType alertType)
        {
            TempData["alert"] = true;
            TempData["alertMessage"] = message;

            var alertClass = String.Empty;

            if (alertType == AlertType.Error)
                alertClass = "alert-error";
            else if (alertType == AlertType.Success)
                alertClass = "alert-success";
            else if (alertType == AlertType.Warning)
                alertClass = "alert-warning";
            else if (alertType == AlertType.Info)
                alertClass = "alert-info";

            TempData["alertClass"] = alertClass;
        }
    }
}