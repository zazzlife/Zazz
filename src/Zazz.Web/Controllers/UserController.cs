using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure;
using Zazz.Infrastructure.Helpers;
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
        private readonly ITagService _tagService;

        public UserController(IStaticDataRepository staticDataRepo, IUoW uow, IPhotoService photoService,
            IUserService userService, ICacheService cacheService, ITagService tagService)
        {
            _staticDataRepo = staticDataRepo;
            _uow = uow;
            _photoService = photoService;
            _userService = userService;
            _cacheService = cacheService;
            _tagService = tagService;
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
            var user = _uow.UserRepository.GetById(id);

            var currentUserId = 0;
            var currentUserPhoto = DefaultImageHelper.GetUserDefaultImage(Gender.NotSpecified);
            var currentUserDisplayName = String.Empty;
            if (User.Identity.IsAuthenticated)
            {
                currentUserId = _userService.GetUserId(User.Identity.Name);
                currentUserPhoto = _photoService.GetUserImageUrl(currentUserId);
                currentUserDisplayName = _userService.GetUserDisplayName(currentUserId);
            }

            if (user.AccountType == AccountType.User)
            {
                return LoadUserProfile(user, currentUserId, currentUserDisplayName, currentUserPhoto);
            }
            else
            {
                return LoadClubProfile(user, currentUserId, currentUserDisplayName, currentUserPhoto);
            }
        }

        private ActionResult LoadClubProfile(User user, int currentUserId,
            string currentUserDisplayName, PhotoLinks currentUserPhoto)
        {
            var weeklies = user.Weeklies.ToList();
            var displayName = _userService.GetUserDisplayName(user.Id);
            var profilePhotoUrl = _photoService.GetUserImageUrl(user.Id);

            var vm = new ClubProfileViewModel
                     {
                         UserId = user.Id,
                         UserName = displayName,
                         CurrentUserDisplayName = currentUserDisplayName,
                         CurrentUserPhoto = currentUserPhoto,
                         UserPhoto = profilePhotoUrl,
                         IsSelf = currentUserId == user.Id,
                         Address = user.ClubDetail.Address,
                         ClubType = user.ClubDetail.ClubType.Name,
                         CoverPhotoUrl = user.ClubDetail.CoverPhotoId.HasValue
                         ? _photoService.GeneratePhotoUrl(user.Id, user.ClubDetail.CoverPhotoId.Value).OriginalLink
                         : DefaultImageHelper.GetDefaultCoverImage(),

                         FollowersCount = _uow.FollowRepository.GetFollowersCount(user.Id),
                         SpecialEventsCount = _uow.EventRepository.GetUpcomingEventsCount(user.Id),
                         IsCurrentUserFollowingTheClub = (currentUserId == user.Id) || currentUserId == 0 ? false : _uow.FollowRepository.Exists(currentUserId, user.Id),
                         Feeds = new FeedHelper(_uow, _userService, _photoService).GetUserActivityFeed(user.Id, currentUserId),
                         Weeklies = weeklies.Select(w => new WeeklyViewModel
                         {
                             DayOfTheWeek = w.DayOfTheWeek,
                             Description = w.Description,
                             Id = w.Id,
                             Name = w.Name,
                             PhotoId = w.PhotoId,
                             OwnerUserId = w.UserId,
                             CurrentUserId = currentUserId,
                             PhotoLinks = w.PhotoId.HasValue
                             ? _photoService.GeneratePhotoUrl(user.Id, w.PhotoId.Value)
                             : DefaultImageHelper.GetDefaultWeeklyImage()
                         }),
                         PartyAlbums = _uow.AlbumRepository.GetLatestAlbums(user.Id)
                           .Select(a => new PartyAlbumViewModel
                           {
                               AlbumId = a.Id,
                               AlbumName = a.Name,
                               CreatedDate = a.CreatedDate,
                               Photos = a.Photos.Select(p => new PhotoViewModel
                               {
                                   FromUserDisplayName = displayName,
                                   FromUserId = user.Id,
                                   FromUserPhotoUrl = profilePhotoUrl,
                                   IsFromCurrentUser = currentUserId == user.Id,
                                   PhotoDescription = p.Description,
                                   PhotoId = p.Id,
                                   PhotoUrl = p.IsFacebookPhoto
                                   ? new PhotoLinks(p.FacebookLink)
                                   : _photoService.GeneratePhotoUrl(p.UserId, p.Id)
                               })
                           })
                     };

            return View("ClubProfile", vm);
        }

        private ActionResult LoadUserProfile(User user, int currentUserId,
            string currentUserDisplayName, PhotoLinks currentUserPhoto)
        {
            var displayName = _userService.GetUserDisplayName(user.Id);
            var profilePhotoUrl = _photoService.GetUserImageUrl(user.Id);

            const int PHOTOS_COUNT = 15;
            var photos = _uow.PhotoRepository.GetLatestUserPhotos(user.Id, PHOTOS_COUNT).ToList();
            var tagStats = _tagService.GetAllTagStats().ToList();

            var vm = new UserProfileViewModel
                     {
                         UserId = user.Id,
                         UserName = displayName,
                         CurrentUserDisplayName = currentUserDisplayName,
                         CurrentUserPhoto = currentUserPhoto,
                         UserPhoto = profilePhotoUrl,
                         IsSelf = currentUserId == user.Id,
                         Feeds = new FeedHelper(_uow, _userService, _photoService).GetUserActivityFeed(user.Id, 
                         currentUserId),
                         FollowersCount = _uow.FollowRepository.GetFollowersCount(user.Id),
                         Photos = photos.Select(p => new PhotoViewModel
                         {
                             FromUserDisplayName = displayName,
                             FromUserId = user.Id,
                             FromUserPhotoUrl = profilePhotoUrl,
                             IsFromCurrentUser = currentUserId == user.Id,
                             PhotoDescription = p.Description,
                             PhotoId = p.Id,
                             PhotoUrl = p.IsFacebookPhoto
                             ? new PhotoLinks(p.FacebookLink)
                             : _photoService.GeneratePhotoUrl(p.UserId, p.Id)
                         }),
                         TagsStats = new TagStatsWidgetViewModel
                         {
                             Tags = tagStats.Select(t => new TagStatViewModel
                             {
                                 TagName = t.Tag.Name,
                                 UsersCount = t.UsersCount
                             }),
                             LastUpdate = tagStats.FirstOrDefault() == null
                                              ? DateTime.MinValue
                                              : tagStats.First().LastUpdate
                         },
                         City = user.UserDetail.City == null ? null : user.UserDetail.City.Name,
                         Major = user.UserDetail.Major == null ? null : user.UserDetail.Major.Name,
                         School = user.UserDetail.School == null ? null : user.UserDetail.School.Name
                     };

            if (!vm.IsSelf && currentUserId != 0)
            {
                vm.IsCurrentUserFollowingTargetUser = _uow.FollowRepository.Exists(currentUserId, user.Id);
                vm.IsTargetUserFollowingCurrentUser = _uow.FollowRepository.Exists(user.Id, currentUserId);
                vm.FollowRequestAlreadySent = _uow.FollowRequestRepository.Exists(currentUserId, user.Id);
            }

            return View("UserProfile", vm);
        }

        public ActionResult LoadMoreFeeds(int lastFeedId)
        {
            var currentUserId = 0;
            if (Request.IsAuthenticated)
                currentUserId = _userService.GetUserId(User.Identity.Name);

            var user = _userService.GetUser(User.Identity.Name);
            var feeds = new FeedHelper(_uow, _userService, _photoService)
                                  .GetUserActivityFeed(user.Id, currentUserId, lastFeedId);

            return View("_FeedsPartial", feeds);
        }

        [HttpGet, Authorize]
        public ActionResult Edit()
        {
            var user = _uow.UserRepository.GetByUsername(User.Identity.Name);

            var vm = new EditUserProfileViewModel
            {
                CurrentUserDisplayName = _userService.GetUserDisplayName(user.Id),
                CurrentUserPhoto = _photoService.GetUserImageUrl(user.Id),
                Gender = user.UserDetail.Gender,
                FullName = user.UserDetail.FullName,
                CityId = user.UserDetail.CityId,
                Cities = new SelectList(_staticDataRepo.GetCities(), "id", "name"),
                SchoolId = user.UserDetail.SchoolId,
                Schools = new SelectList(_staticDataRepo.GetSchools(), "id", "name"),
                MajorId = user.UserDetail.MajorId,
                Majors = new SelectList(_staticDataRepo.GetMajors(), "id", "name"),
                SendFbErrorNotification = user.Preferences.SendSyncErrorNotifications,
                SyncFbEvents = user.Preferences.SyncFbEvents,
                //SyncFbPosts = user.UserDetail.SyncFbPosts,
                //SyncFbImages = user.UserDetail.SyncFbImages,
                //AccountType = user.AccountType
            };

            return View("EditUser", vm);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult Edit(EditUserProfileViewModel vm)
        {
            var user = _uow.UserRepository.GetByUsername(User.Identity.Name);

            vm.Cities = new SelectList(_staticDataRepo.GetCities(), "id", "name");
            vm.Schools = new SelectList(_staticDataRepo.GetSchools(), "id", "name");
            vm.Majors = new SelectList(_staticDataRepo.GetMajors(), "id", "name");

            vm.CurrentUserDisplayName = _userService.GetUserDisplayName(user.Id);
            vm.CurrentUserPhoto = _photoService.GetUserImageUrl(user.Id);

            if (ModelState.IsValid)
            {
                user.UserDetail.Gender = vm.Gender;
                user.UserDetail.FullName = vm.FullName;
                user.UserDetail.SchoolId = (short)vm.SchoolId;
                user.UserDetail.CityId = (short)vm.CityId;
                user.UserDetail.MajorId = (byte)vm.MajorId;
                user.Preferences.SyncFbEvents = vm.SyncFbEvents;
                //user.UserDetail.SyncFbPosts = vm.SyncFbPosts;
                //user.UserDetail.SyncFbImages = vm.SyncFbImages;
                user.Preferences.SendSyncErrorNotifications = vm.SendFbErrorNotification;

                _uow.SaveChanges();

                _cacheService.RemoveUserDisplayName(user.Id);
                ShowAlert("Your preferences has been updated.", AlertType.Success);
            }

            return View("EditUser", vm);
        }

        [Authorize]
        public void ChangeProfilePic(int id)
        {
            var user = _uow.UserRepository.GetByUsername(User.Identity.Name);
            user.ProfilePhotoId = id;

            _uow.SaveChanges();
            _cacheService.RemoveUserPhotoUrl(user.Id);
        }

        [Authorize]
        public void ChangeCoverPic(int id)
        {
            var user = _uow.UserRepository.GetByUsername(User.Identity.Name);
            if (user.AccountType != AccountType.ClubAdmin)
                throw new SecurityException();

            user.ClubDetail.CoverPhotoId = id;
            _uow.SaveChanges();
        }

        public string GetCurrentUserImageUrl()
        {
            if (!User.Identity.IsAuthenticated)
                return DefaultImageHelper.GetUserDefaultImage(Gender.NotSpecified).SmallLink;

            var userId = _userService.GetUserId(User.Identity.Name);
            return _photoService.GetUserImageUrl(userId).SmallLink;
        }

        public string GetUserImageUrl(int userId)
        {
            return _photoService.GetUserImageUrl(userId).VerySmallLink;
        }

        public string GetCurrentUserDisplayName()
        {
            return _userService.GetUserDisplayName(User.Identity.Name);
        }

        public string GetUserDisplayName(int userId)
        {
            return _userService.GetUserDisplayName(userId);
        }
    }
}
