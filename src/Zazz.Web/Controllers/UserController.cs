using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class UserController : BaseController
    {
        private readonly IStaticDataRepository _staticDataRepo;
        private readonly IUserService _userService;

        public UserController(IStaticDataRepository staticDataRepo, IUserService userService)
        {
            _staticDataRepo = staticDataRepo;
            _userService = userService;
        }

        public ActionResult Index()
        {
            return Me();
        }

        public ActionResult Me()
        {
            return View("Profile");
        }

        [HttpGet, Authorize]
        public async Task<ActionResult> Edit()
        {
            using (_userService)
            {
                var user = await _userService.GetUserAsync(User.Identity.Name);

                var vm = new EditProfileViewModel
                {
                    FullName = user.FullName,
                    Cities = new SelectList(_staticDataRepo.GetCities(), "id", "name", user.CityId),
                    Schools = new SelectList(_staticDataRepo.GetSchools(), "id", "name", user.SchoolId),
                    Majors = new SelectList(_staticDataRepo.GetMajors(), "id", "name", user.MajorId),
                    SendFbErrorNotification = user.UserDetail.SendSyncErrorNotifications,
                    SyncFbEvents = user.UserDetail.SyncFbEvents,
                    SyncFbPosts = user.UserDetail.SyncFbPosts,
                    SyncFbImages = user.UserDetail.SyncFbImages
                };

                return View(vm);
            }
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult Edit(EditProfileViewModel vm)
        {
            return View();
        }
    }
}
