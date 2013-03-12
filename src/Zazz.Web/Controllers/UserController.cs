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
        private readonly IUoW _uow;

        public UserController(IStaticDataRepository staticDataRepo, IUoW uow)
        {
            _staticDataRepo = staticDataRepo;
            _uow = uow;
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
            using (_uow)
            {
                var user = await _uow.UserRepository.GetByUsernameAsync(User.Identity.Name);

                var vm = new EditProfileViewModel
                {
                    Gender = user.UserDetail.Gender,
                    FullName = user.UserDetail.FullName,
                    Cities = new SelectList(_staticDataRepo.GetCities(), "id", "name", user.UserDetail.CityId),
                    Schools = new SelectList(_staticDataRepo.GetSchools(), "id", "name", user.UserDetail.SchoolId),
                    Majors = new SelectList(_staticDataRepo.GetMajors(), "id", "name", user.UserDetail.MajorId),
                    SendFbErrorNotification = user.UserDetail.SendSyncErrorNotifications,
                    SyncFbEvents = user.UserDetail.SyncFbEvents,
                    SyncFbPosts = user.UserDetail.SyncFbPosts,
                    SyncFbImages = user.UserDetail.SyncFbImages
                };

                return View(vm);
            }
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditProfileViewModel vm)
        {
            vm.Cities = new SelectList(_staticDataRepo.GetCities(), "id", "name", vm.CityId);
            vm.Schools = new SelectList(_staticDataRepo.GetSchools(), "id", "name", vm.SchoolId);
            vm.Majors = new SelectList(_staticDataRepo.GetMajors(), "id", "name", vm.MajorId);

            if (ModelState.IsValid)
            {
                using (_uow)
                {
                    var user = await _uow.UserRepository.GetByUsernameAsync(User.Identity.Name);   

                    string message;
                    if (vm.ProfileImage != null)
                    {
                        if (!ImageValidator.IsValid(vm.ProfileImage, out message))
                        {
                            ShowAlert(message, AlertType.Error);
                            return View(vm);
                        }
                    }

                    if (vm.ProfileCoverImage != null)
                    {
                        if (!ImageValidator.IsValid(vm.ProfileCoverImage, out message))
                        {
                            ShowAlert(message, AlertType.Error);
                            return View(vm);
                        }
                    }
                }
            }

            return View(vm);
        }
    }
}
