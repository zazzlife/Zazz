using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class NotificationController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;

        public NotificationController(IUserService userService, IPhotoService photoService)
        {
            _userService = userService;
            _photoService = photoService;
        }

        [Authorize]
        public ActionResult Index()
        {
            var vm = new UserHomeViewModel
                     {
                         CurrentUserDisplayName = User.Identity.Name,
                         AccountType = AccountType.User,
                         CurrentUserPhoto = _photoService.GetUserImageUrl(1)
                     };

            return View(vm);
        }

        public ActionResult Get(int id, int? lastNotification)
        {
            return View("_Notifications");
        }
    }
}
