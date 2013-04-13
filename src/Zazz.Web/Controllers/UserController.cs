using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure;
using Zazz.Web.Helpers;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class UserController : BaseController
    {
        private readonly IStaticDataRepository _staticDataRepo;
        private readonly IUoW _uow;
        private readonly IPhotoService _photoService;
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;

        public UserController(IStaticDataRepository staticDataRepo, IUoW uow, IPhotoService photoService,
            IUserService userService, ICacheService cacheService)
        {
            _staticDataRepo = staticDataRepo;
            _uow = uow;
            _photoService = photoService;
            _userService = userService;
            _cacheService = cacheService;
        }

        [Authorize]
        public ActionResult Index()
        {
            var userId = _userService.GetUserId(User.Identity.Name);
            return ShowProfile(userId);
        }

        [ActionName("Profile")]
        public ActionResult ShowProfile(int id)
        {
            using (_uow)
            using (_photoService) 
            using (_userService)
            {
                var user = _uow.UserRepository.GetById(id);
                
                // Profile Photo
                var profilePhotoUrl = _photoService.GetUserImageUrl(id);

                // Cover Photo
                string coverPhotoUrl;
                if (user.UserDetail.CoverPhotoId == 0)
                {
                    coverPhotoUrl = DefaultImageHelper.GetDefaultCoverImage();
                }
                else
                {
                    var photo = _uow.PhotoRepository.GetPhotoWithMinimalData(user.UserDetail.CoverPhotoId);
                    if (photo == null)
                    {
                        coverPhotoUrl = DefaultImageHelper.GetDefaultCoverImage();
                    }
                    else
                    {
                        if (photo.IsFacebookPhoto)
                        {
                            coverPhotoUrl = photo.FacebookPicUrl;
                        }
                        else
                        {
                            coverPhotoUrl = _photoService.GeneratePhotoUrl(id, photo.Id).OriginalLink;
                        }
                    }
                }

                // User Name
                var username = String.IsNullOrEmpty(user.UserDetail.FullName)
                                      ? user.Username
                                      : user.UserDetail.FullName;

                var currentUserId = 0;
                if (User.Identity.IsAuthenticated)
                {
                    currentUserId = _userService.GetUserId(User.Identity.Name);
                }

                // Feeds 
                var feedsHelper = new FeedHelper(_uow, _userService, _photoService);

                var vm = new UserProfileViewModel
                         {
                             UserPhotoUrl = profilePhotoUrl,
                             CoverPhotoUrl = coverPhotoUrl,
                             UserName = username,
                             IsSelf = user.Id == currentUserId,
                             FollowersCount = _uow.FollowRepository.GetFollowersCount(id),
                             AccountType = user.AccountType,
                             UserId = id,
                             IsClub = user.AccountType == AccountType.ClubAdmin,
                             Feeds = feedsHelper.GetUserActivityFeed(user.Id, currentUserId)
                         };

                if (!vm.IsSelf && currentUserId != 0)
                {
                    vm.IsCurrentUserFollowingTargetUser = _uow.FollowRepository.Exists(currentUserId, id);
                    vm.IsTargetUserFollowingCurrentUser = _uow.FollowRepository.Exists(id, currentUserId);

                    if (!vm.IsCurrentUserFollowingTargetUser)
                    {
                        vm.FollowRequestAlreadySent = _uow.FollowRequestRepository
                            .Exists(currentUserId, id);
                    }
                }

                if (user.UserDetail.City != null)
                    vm.City = user.UserDetail.City.Name;

                if (user.UserDetail.School != null)
                    vm.School = user.UserDetail.School.Name;

                if (user.UserDetail.Major != null)
                    vm.Major = user.UserDetail.Major.Name;

                return View("Profile", vm);
            }
        }

        public ActionResult LoadMoreFeeds(int lastFeedId)
        {
            using (_uow)
            using (_photoService)
            using (_userService)
            {
                var currentUserId = 0;
                if (Request.IsAuthenticated)
                    currentUserId = _userService.GetUserId(User.Identity.Name);

                var user = _userService.GetUser(User.Identity.Name);
                var feeds = new FeedHelper(_uow, _userService, _photoService)
                                      .GetUserActivityFeed(user.Id, currentUserId, lastFeedId);

                return View("_FeedsPartial", feeds);
            }
        }

        [HttpGet, Authorize]
        public ActionResult Edit()
        {
            using (_uow)
            using (_userService)
            using (_photoService)
            {
                var user = _uow.UserRepository.GetByUsername(User.Identity.Name);

                var vm = new EditProfileViewModel
                {
                    Gender = user.UserDetail.Gender,
                    FullName = user.UserDetail.FullName,
                    CityId = user.UserDetail.CityId,
                    Cities = new SelectList(_staticDataRepo.GetCities(), "id", "name"),
                    SchoolId = user.UserDetail.SchoolId,
                    Schools = new SelectList(_staticDataRepo.GetSchools(), "id", "name"),
                    MajorId = user.UserDetail.MajorId,
                    Majors = new SelectList(_staticDataRepo.GetMajors(), "id", "name"),
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
        public ActionResult Edit(EditProfileViewModel vm)
        {
            using (_uow)
            using (_userService)
            using (_photoService)
            {
                var user = _uow.UserRepository.GetByUsername(User.Identity.Name);

                vm.Cities = new SelectList(_staticDataRepo.GetCities(), "id", "name");
                vm.Schools = new SelectList(_staticDataRepo.GetSchools(), "id", "name");
                vm.Majors = new SelectList(_staticDataRepo.GetMajors(), "id", "name");
                vm.Albums = new SelectList(user.Albums.ToList(), "id", "name");

                if (ModelState.IsValid)
                {
                    user.UserDetail.Gender = vm.Gender;
                    user.UserDetail.FullName = vm.FullName;
                    user.UserDetail.SchoolId = (short)vm.SchoolId;
                    user.UserDetail.CityId = (short)vm.CityId;
                    user.UserDetail.MajorId = (byte)vm.MajorId;
                    user.UserDetail.SyncFbEvents = vm.SyncFbEvents;
                    user.UserDetail.SyncFbPosts = vm.SyncFbPosts;
                    user.UserDetail.SyncFbImages = vm.SyncFbImages;
                    user.UserDetail.SendSyncErrorNotifications = vm.SendFbErrorNotification;

                    _uow.SaveChanges();

                    _cacheService.RemoveUserDisplayName(user.Id);
                    ShowAlert("Your preferences has been updated.", AlertType.Success);
                }
            }

            return View(vm);
        }

        [Authorize]
        public void ChangeProfilePic(int id)
        {
            using (_uow)
            using (_userService)
            using (_photoService)
            {
                var user = _uow.UserRepository.GetByUsername(User.Identity.Name);
                user.UserDetail.ProfilePhotoId = id;

                _uow.SaveChanges();
                _cacheService.RemoveUserPhotoUrl(user.Id);
            }
        }

        [Authorize]
        public void ChangeCoverPic(int id)
        {
            using (_uow)
            using (_userService)
            using (_photoService)
            {
                var user = _uow.UserRepository.GetByUsername(User.Identity.Name);
                user.UserDetail.CoverPhotoId = id;

                _uow.SaveChanges();
            }
        }

        public string GetCurrentUserImageUrl()
        {
            using (_uow)
            using (_userService)
            using (_photoService)
            {
                if (!User.Identity.IsAuthenticated)
                    return DefaultImageHelper.GetUserDefaultImage(Gender.NotSpecified).SmallLink;

                var userId = _userService.GetUserId(User.Identity.Name);
                return _photoService.GetUserImageUrl(userId).SmallLink;
            }
        }

        public string GetUserImageUrl(int userId)
        {
            using (_uow)
            using (_userService)
            using (_photoService)
            {
                return _photoService.GetUserImageUrl(userId).VerySmallLink;
            }
        }

        public string GetCurrentUserDisplayName()
        {
            using (_uow)
            using (_userService)
            using (_photoService)
            {
                return _userService.GetUserDisplayName(User.Identity.Name);
            }
        }

        public string GetUserDisplayName(int userId)
        {
            using (_uow)
            using (_userService)
            using (_photoService)
            {
                return _userService.GetUserDisplayName(userId);
            }
        }
    }
}
