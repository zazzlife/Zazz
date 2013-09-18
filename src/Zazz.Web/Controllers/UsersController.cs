using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure;
using Zazz.Infrastructure.Helpers;
using Zazz.Web.Helpers;
using Zazz.Web.Interfaces;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class UsersController : BaseController
    {
        private readonly IUoW _uow;
        private readonly ICacheService _cacheService;
        private readonly IFeedHelper _feedHelper;

        public UsersController(IStaticDataRepository staticDataRepo, IUoW uow, IPhotoService photoService,
            IUserService userService, ICacheService cacheService, ICategoryService categoryService,
            IDefaultImageHelper defaultImageHelper, IFeedHelper feedHelper)
            : base(userService, photoService, defaultImageHelper, staticDataRepo, categoryService)
        {
            _uow = uow;
            _cacheService = cacheService;
            _feedHelper = feedHelper;
        }

        [Authorize]
        public ActionResult Index()
        {
            var userId = GetCurrentUserId();
            var displayName = GetCurrentUserDisplayName();

            return RedirectToAction("Profile", "Users",
                                    new {id = userId, friendlySeoName = displayName.ToUrlFriendlyString()});
        }

        [ActionName("Profile")]
        public ActionResult ShowProfile(int id, string friendlySeoName)
        {
            var displayName = UserService.GetUserDisplayName(id);
            if (String.IsNullOrEmpty(displayName))
                throw new HttpException(404, "user not found");

            //SEO
            var realFriendlySeoName = displayName.ToUrlFriendlyString();
            if (!realFriendlySeoName.Equals(friendlySeoName))
                return RedirectToActionPermanent("Profile", "Users", new { id, friendlySeoName = realFriendlySeoName });


            var user = _uow.UserRepository.GetById(id, true, true, true);

            var profilePhotoUrl = PhotoService.GetUserDisplayPhoto(user.Id);

            var currentUserId = 0;
            if (User.Identity.IsAuthenticated)
            {
                currentUserId = UserService.GetUserId(User.Identity.Name);
            }

            if (user.AccountType == AccountType.User)
            {
                return LoadUserProfile(user, currentUserId, displayName, profilePhotoUrl);
            }
            else
            {
                return LoadClubProfile(user, currentUserId, displayName, profilePhotoUrl);
            }
        }

        private ActionResult LoadClubProfile(User user, int currentUserId, string displayName, PhotoLinks profilePhotoUrl)
        {
            var weeklies = user.Weeklies.ToList();

            var vm = new ClubProfileViewModel
                     {
                         UserId = user.Id,
                         UserName = displayName,
                         UserPhoto = profilePhotoUrl,
                         IsSelf = currentUserId == user.Id,
                         Address = user.ClubDetail.Address,
                         ClubType = user.ClubDetail.ClubType,
                         CoverPhotoUrl = user.ClubDetail.CoverPhotoId.HasValue
                         ? PhotoService.GeneratePhotoUrl(user.Id, user.ClubDetail.CoverPhotoId.Value).OriginalLink
                         : DefaultImageHelper.GetDefaultCoverImage().OriginalLink,

                         FollowersCount = _uow.FollowRepository.GetFollowersCount(user.Id),
                         Events = _uow.EventRepository.GetUpcomingEvents(user.Id).ToList()
                            .Select(e => new EventViewModel
                                          {
                                              City = e.City,
                                              Id = e.Id,
                                              CreatedDate = e.CreatedDate,
                                              Description = e.Description,
                                              Latitude = e.Latitude,
                                              Longitude = e.Longitude,
                                              Location = e.Location,
                                              Name = e.Name,
                                              Price = e.Price,
                                              Street = e.Street,
                                              Time = e.Time,
                                              PhotoId = e.PhotoId,
                                              IsFacebookEvent = e.IsFacebookEvent,
                                              FacebookPhotoUrl = e.FacebookPhotoLink,
                                              IsDateOnly = e.IsDateOnly,
                                              FacebookEventId = e.FacebookEventId,
                                              ImageUrl = e.IsFacebookEvent 
                                                ? new PhotoLinks(e.FacebookPhotoLink) 
                                                : e.PhotoId.HasValue
                                                    ? PhotoService.GeneratePhotoUrl(e.UserId, e.PhotoId.Value)
                                                    : DefaultImageHelper.GetDefaultEventImage()
                                          })
                             .ToList(),
                         IsCurrentUserFollowingTheClub = (currentUserId == user.Id) || currentUserId == 0 ? false : _uow.FollowRepository.Exists(currentUserId, user.Id),
                         Feeds = _feedHelper.GetUserActivityFeed(user.Id, currentUserId),
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
                             ? PhotoService.GeneratePhotoUrl(user.Id, w.PhotoId.Value)
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
                                   Description = p.Description,
                                   PhotoId = p.Id,
                                   PhotoUrl = PhotoService.GeneratePhotoUrl(p.UserId, p.Id)
                               })
                           })
                     };

            return View("ClubProfile", vm);
        }

        private ActionResult LoadUserProfile(User user, int currentUserId, string displayName, PhotoLinks profilePhotoUrl)
        {
            const int PHOTOS_COUNT = 15;
            var photos = _uow.PhotoRepository.GetLatestUserPhotos(user.Id, PHOTOS_COUNT).ToList();

            var vm = new UserProfileViewModel
                     {
                         UserId = user.Id,
                         UserName = displayName,
                         UserPhoto = profilePhotoUrl,
                         IsSelf = currentUserId == user.Id,
                         Feeds = _feedHelper
                         .GetUserActivityFeed(user.Id,
                         currentUserId),
                         FollowersCount = _uow.FollowRepository.GetFollowersCount(user.Id),
                         FollowingsCount = _uow.FollowRepository.GetUserFollows(user.Id).Count(),
                         ReceivedVotesCount = _uow.PhotoVoteRepository.GetReceivedVotes(user.Id).Count(),
                         Photos = photos.Select(p => new PhotoViewModel
                         {
                             FromUserDisplayName = displayName,
                             FromUserId = user.Id,
                             FromUserPhotoUrl = profilePhotoUrl,
                             IsFromCurrentUser = currentUserId == user.Id,
                             Description = p.Description,
                             PhotoId = p.Id,
                             PhotoUrl = PhotoService.GeneratePhotoUrl(p.UserId, p.Id)
                         }),
                         CategoriesStats = GetTagStats(),
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
                currentUserId = UserService.GetUserId(User.Identity.Name);

            var user = UserService.GetUser(User.Identity.Name);
            var feeds = _feedHelper.GetUserActivityFeed(user.Id, currentUserId, lastFeedId);

            return View("_FeedsPartial", feeds);
        }

        [HttpGet, Authorize]
        public ActionResult Edit()
        {
            var user = _uow.UserRepository.GetByUsername(User.Identity.Name, true, true, false, true);
            return user.AccountType == AccountType.User ? EditUser(user) : EditClub(user);
        }

        private ActionResult EditUser(User user)
        {
            var vm = new EditUserProfileViewModel
                     {
                         Gender = user.UserDetail.Gender,
                         FullName = user.UserDetail.FullName,
                         CityId = user.UserDetail.CityId,
                         Cities = StaticDataRepository.GetCities(),
                         SchoolId = user.UserDetail.SchoolId,
                         Schools = StaticDataRepository.GetSchools(),
                         MajorId = user.UserDetail.MajorId,
                         Majors = StaticDataRepository.GetMajors(),
                         SendFbErrorNotification = user.Preferences.SendSyncErrorNotifications,
                         SyncFbEvents = user.Preferences.SyncFbEvents
                     };

            return View("EditUser", vm);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult EditUser(EditUserProfileViewModel vm)
        {
            var user = _uow.UserRepository.GetByUsername(User.Identity.Name, true, false, false, true);
            if (user.AccountType != AccountType.User)
                throw new SecurityException();

            vm.Cities = StaticDataRepository.GetCities();
            vm.Schools = StaticDataRepository.GetSchools();
            vm.Majors = StaticDataRepository.GetMajors();

            if (ModelState.IsValid)
            {
                user.UserDetail.Gender = vm.Gender;
                user.UserDetail.FullName = vm.FullName;
                user.UserDetail.SchoolId = vm.SchoolId;
                user.UserDetail.CityId = vm.CityId;
                user.UserDetail.MajorId = vm.MajorId;
                user.Preferences.SyncFbEvents = vm.SyncFbEvents;
                user.Preferences.SendSyncErrorNotifications = vm.SendFbErrorNotification;

                _uow.SaveChanges();

                _cacheService.RemoveUserDisplayName(user.Id);
                ShowAlert("Your preferences has been updated.", AlertType.Success);
            }

            return View("EditUser", vm);
        }

        private ActionResult EditClub(User user)
        {
            var vm = new EditClubProfileViewModel
                     {
                         ClubAddress = user.ClubDetail.Address,
                         ClubName = user.ClubDetail.ClubName,
                         ClubType = user.ClubDetail.ClubType,
                         SendFbErrorNotification = user.Preferences.SendSyncErrorNotifications,
                         SyncFbEvents = user.Preferences.SyncFbEvents,
                         SyncFbImages = user.Preferences.SyncFbImages,
                         SyncFbPosts = user.Preferences.SyncFbPosts
                     };

            return View("EditClub", vm);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult EditClub(EditClubProfileViewModel vm)
        {
            var user = _uow.UserRepository.GetByUsername(User.Identity.Name, false, true, false, true);
            if (user.AccountType != AccountType.Club)
                throw new SecurityException();

            if (ModelState.IsValid)
            {
                user.ClubDetail.Address = vm.ClubAddress;
                user.ClubDetail.ClubName = vm.ClubName;
                user.ClubDetail.ClubType = vm.ClubType;
                user.Preferences.SendSyncErrorNotifications = vm.SendFbErrorNotification;
                user.Preferences.SyncFbEvents = vm.SyncFbEvents;
                user.Preferences.SyncFbImages = vm.SyncFbImages;
                user.Preferences.SyncFbPosts = vm.SyncFbPosts;

                _uow.SaveChanges();

                ShowAlert("Your preferences has been updated.", AlertType.Success);
            }

            return View("EditClub", vm);
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
            if (user.AccountType != AccountType.Club)
                throw new SecurityException();

            user.ClubDetail.CoverPhotoId = id;
            _uow.SaveChanges();
        }
    }
}
