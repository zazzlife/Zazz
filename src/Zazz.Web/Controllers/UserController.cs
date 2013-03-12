using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class UserController : BaseController
    {
        private readonly IStaticDataRepository _staticDataRepo;
        private readonly IUoW _uow;
        private readonly IPhotoService _photoService;

        public UserController(IStaticDataRepository staticDataRepo, IUoW uow, IPhotoService photoService)
        {
            _staticDataRepo = staticDataRepo;
            _uow = uow;
            _photoService = photoService;
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
                    Albums = new SelectList(user.Albums.ToList(), "id", "name"),
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
            using (_uow)
            {
                var user = await _uow.UserRepository.GetByUsernameAsync(User.Identity.Name);

                vm.Cities = new SelectList(_staticDataRepo.GetCities(), "id", "name", vm.CityId);
                vm.Schools = new SelectList(_staticDataRepo.GetSchools(), "id", "name", vm.SchoolId);
                vm.Majors = new SelectList(_staticDataRepo.GetMajors(), "id", "name", vm.MajorId);
                vm.Albums = new SelectList(user.Albums.ToList(), "id", "name");

                if (ModelState.IsValid)
                {
                    string message;
                    if (vm.ProfileImage != null)
                    {
                        using (vm.ProfileImage.InputStream)
                        {
                            if (!ImageValidator.IsValid(vm.ProfileImage, out message))
                            {
                                ShowAlert(message, AlertType.Error);
                                return View(vm);
                            }

                            var photo = new Photo
                            {
                                AlbumId = vm.AlbumId,
                                UploaderId = user.Id
                            };

                            var photoId = await _photoService.SavePhotoAsync(photo, vm.ProfileImage.InputStream);
                            user.UserDetail.ProfilePhotoId = photoId;
                        }
                    }

                    if (vm.ProfileCoverImage != null)
                    {
                        using (vm.ProfileCoverImage.InputStream)
                        {
                            if (!ImageValidator.IsValid(vm.ProfileCoverImage, out message))
                            {
                                ShowAlert(message, AlertType.Error);
                                return View(vm);
                            }

                            var photo = new Photo
                            {
                                AlbumId = vm.AlbumId,
                                UploaderId = user.Id
                            };

                            var photoId = await _photoService.SavePhotoAsync(photo, vm.ProfileCoverImage.InputStream);
                            user.UserDetail.CoverPhotoId = photoId;
                        }
                    }

                    user.UserDetail.Gender = vm.Gender;
                    user.UserDetail.FullName = vm.FullName;
                    user.UserDetail.SchoolId = (short)vm.SchoolId;
                    user.UserDetail.CityId = (short) vm.CityId;
                    user.UserDetail.MajorId = (byte) vm.MajorId;
                    user.UserDetail.SyncFbEvents = vm.SyncFbEvents;
                    user.UserDetail.SyncFbPosts = vm.SyncFbPosts;
                    user.UserDetail.SyncFbImages = vm.SyncFbImages;
                    user.UserDetail.SendSyncErrorNotifications = vm.SendFbErrorNotification;

                    await _uow.SaveAsync();
                    ShowAlert("Your preferences has been updated.", AlertType.Success);
                }
            }

            return View(vm);
        }
    }
}
