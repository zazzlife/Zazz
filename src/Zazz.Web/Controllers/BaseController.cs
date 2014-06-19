using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        public IStaticDataRepository StaticDataRepository { get; set; }
        protected readonly IUserService UserService;
        protected readonly IPhotoService PhotoService;
        protected readonly IDefaultImageHelper DefaultImageHelper;
        protected readonly ICategoryService CategoryService;

        protected BaseController(IUserService userService, IPhotoService photoService,
            IDefaultImageHelper defaultImageHelper, IStaticDataRepository staticDataRepository,
            ICategoryService categoryService)
        {
            StaticDataRepository = staticDataRepository;
            UserService = userService;
            PhotoService = photoService;
            DefaultImageHelper = defaultImageHelper;
            CategoryService = categoryService;
        }

        public IEnumerable<CategoryStatViewModel> GetTagStats()
        {
            return CategoryService.GetAllStats()
                             .Select(t => new CategoryStatViewModel
                             {
                                 CategoryName = t.Category.Name,
                                 UsersCount = t.UsersCount
                             });
        }

        public string GetCurrentUserDisplayName()
        {
            if (!User.Identity.IsAuthenticated)
                return "Not Logged in!";

            return UserService.GetUserDisplayName(User.Identity.Name);
        }

        public int GetCurrentUserId()
        {
            var id = 0;

            if (User.Identity.IsAuthenticated)
                id = UserService.GetUserId(User.Identity.Name);

            return id;
        }

        public string GetDisplayName(int id)
        {
            return UserService.GetUserDisplayName(id);
        }

        public string GetCurrentUserVerySmallDisplayPicture()
        {
            return GetDisplayPicture().VerySmallLink;
        }

        public string GetVerySmallDisplayPicture(int userId)
        {
            return GetDisplayPicture(userId).VerySmallLink;
        }

        public PhotoLinks GetDisplayPicture()
        {
            if (!User.Identity.IsAuthenticated)
                return DefaultImageHelper.GetUserDefaultImage(Gender.NotSpecified);

            var userId = UserService.GetUserId(User.Identity.Name);
            return GetDisplayPicture(userId);
        }

        public string GetClubUsernames()
        {
            return UserService.GetClubUsernames();
        }

        private PhotoLinks GetDisplayPicture(int id)
        {
            return PhotoService.GetUserDisplayPhoto(id);
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