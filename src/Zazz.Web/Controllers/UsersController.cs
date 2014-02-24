using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
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
        private readonly IFollowService _followService;

        public UsersController(IStaticDataRepository staticDataRepo, IUoW uow, IPhotoService photoService,
            IUserService userService, ICacheService cacheService, ICategoryService categoryService,
            IDefaultImageHelper defaultImageHelper, IFeedHelper feedHelper, IFollowService followService)
            : base(userService, photoService, defaultImageHelper, staticDataRepo, categoryService)
        {
            _uow = uow;
            _cacheService = cacheService;
            _feedHelper = feedHelper;
            _followService = followService;
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
            var baseVm = LoadBaseClubProfileVm(user, currentUserId, displayName, profilePhotoUrl);
            var vm = new ClubProfileViewModel
                     {
                         Address = baseVm.Address,
                         ClubType = baseVm.ClubType,
                         CoverPhotoUrl = baseVm.CoverPhotoUrl,
                         Events = baseVm.Events,
                         Feeds = _feedHelper.GetUserActivityFeed(user.Id, currentUserId),
                         FollowersCount = baseVm.FollowersCount,
                         FollowingsCount = baseVm.FollowingsCount,
                         IsCurrentUserFollowingTheClub = baseVm.IsCurrentUserFollowingTheClub,
                         IsSelf = baseVm.IsSelf,
                         PartyAlbums = baseVm.PartyAlbums,
                         ReceivedLikesCount = baseVm.ReceivedLikesCount,
                         UserId = baseVm.UserId,
                         UserName = baseVm.UserName,
                         UserPhoto = baseVm.UserPhoto,
                         Weeklies = baseVm.Weeklies
                     };

            return View("ClubProfile", vm);
        }

        private UserProfileViewModelBase LoadBaseUserProfileVm(User user)
        {
            return LoadBaseUserProfileVm(user,
                GetCurrentUserId(),
                UserService.GetUserDisplayName(user.Id),
                PhotoService.GetUserDisplayPhoto(user.Id));
        }

        private UserProfileViewModelBase LoadBaseUserProfileVm(User user, int currentUserId, string displayName,
            PhotoLinks profilePhotoUrl)
        {
            var vm = new UserProfileViewModel
            {
                UserId = user.Id,
                UserName = displayName,
                UserPhoto = profilePhotoUrl,
                IsSelf = currentUserId == user.Id,
                FollowersCount = _uow.FollowRepository.GetUserFollowers(user.Id).Count(),
                FollowingsCount = _uow.FollowRepository.GetUserFollows(user.Id).Count(),
                ReceivedLikesCount = _uow.UserReceivedLikesRepository.GetCount(user.Id),
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

            return vm;
        }

        private ClubProfileViewModelBase LoadBaseClubProfileVm(User user)
        {
            return LoadBaseClubProfileVm(user,
                GetCurrentUserId(),
                UserService.GetUserDisplayName(user.Id),
                PhotoService.GetUserDisplayPhoto(user.Id));
        }

        private ClubProfileViewModelBase LoadBaseClubProfileVm(User user, int currentUserId, string displayName,
            PhotoLinks profilePhotoUrl)
        {
            var weeklies = user.Weeklies.ToList();

            var vm = new ClubProfileViewModelBase
            {
                UserId = user.Id,
                UserName = displayName,
                UserPhoto = profilePhotoUrl,
                IsSelf = currentUserId == user.Id,
                Address = user.ClubDetail.Address,
                ClubType = user.ClubDetail.ClubType,
                FollowersCount = _uow.FollowRepository.GetUserFollowers(user.Id).Count(),
                FollowingsCount = _uow.FollowRepository.GetUserFollows(user.Id).Count(),
                ReceivedLikesCount = _uow.UserReceivedLikesRepository.GetCount(user.Id),
                CoverPhotoUrl = user.ClubDetail.CoverPhotoId.HasValue
                ? PhotoService.GeneratePhotoUrl(user.Id, user.ClubDetail.CoverPhotoId.Value).OriginalLink
                : DefaultImageHelper.GetDefaultCoverImage().OriginalLink,
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
                }).OrderBy(w => w.DayOfTheWeek),
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
                  }).ToList()
            };

            // getting pics that are not in albums

            var pics = _uow.PhotoRepository.GetLatestUserPhotos(user.Id, 10)
                .Where(p => p.AlbumId == null)
                .ToList()
                .Select(p => new PhotoViewModel
                {
                    FromUserDisplayName = displayName,
                    FromUserId = user.Id,
                    FromUserPhotoUrl = profilePhotoUrl,
                    IsFromCurrentUser = currentUserId == user.Id,
                    Description = p.Description,
                    PhotoId = p.Id,
                    PhotoUrl = PhotoService.GeneratePhotoUrl(p.UserId, p.Id)
                }).ToList();

            var otherPics = new PartyAlbumViewModel
            {
                AlbumId = 0,
                AlbumName = "Other pics",
                Photos = pics
            };

            if (otherPics.Photos.Any())
                vm.PartyAlbums.Add(otherPics);

            return vm;
        }

        private ActionResult LoadUserProfile(User user, int currentUserId, string displayName, PhotoLinks profilePhotoUrl)
        {
            var baseVm = LoadBaseUserProfileVm(user, currentUserId, displayName, profilePhotoUrl);
            var vm = new UserProfileViewModel
            {
                CategoriesStats = baseVm.CategoriesStats,
                City = baseVm.City,
                Feeds = _feedHelper
                    .GetUserActivityFeed(user.Id, currentUserId),
                UserId = baseVm.UserId,
                UserName = baseVm.UserName,
                UserPhoto = baseVm.UserPhoto,
                IsSelf = baseVm.IsSelf,
                FollowersCount = baseVm.FollowersCount,
                FollowingsCount = baseVm.FollowingsCount,
                ReceivedLikesCount = baseVm.ReceivedLikesCount,
                Major = baseVm.Major,
                School = baseVm.School,
                FollowRequestAlreadySent = baseVm.FollowRequestAlreadySent,
                IsTargetUserFollowingCurrentUser = baseVm.IsTargetUserFollowingCurrentUser,
                IsCurrentUserFollowingTargetUser = baseVm.IsCurrentUserFollowingTargetUser
            };

            return View("UserProfile", vm);
        }

        public ActionResult Photos(int id)
        {
            var displayName = UserService.GetUserDisplayName(id);

            var user = _uow.UserRepository.GetById(id, true, true, true);
            var profilePhotoUrl = PhotoService.GetUserDisplayPhoto(user.Id);

            var currentUserId = 0;
            if (User.Identity.IsAuthenticated)
            {
                currentUserId = UserService.GetUserId(User.Identity.Name);
            }

            var photos = _uow.PhotoRepository.GetLatestUserPhotos(id, 500).ToList();

            var baseVm = LoadBaseUserProfileVm(user, currentUserId, displayName, profilePhotoUrl);
            var vm = new UserPhotosViewModel
            {
                CategoriesStats = baseVm.CategoriesStats,
                City = baseVm.City,
                UserId = baseVm.UserId,
                UserName = baseVm.UserName,
                UserPhoto = baseVm.UserPhoto,
                IsSelf = baseVm.IsSelf,
                FollowersCount = baseVm.FollowersCount,
                FollowingsCount = baseVm.FollowingsCount,
                ReceivedLikesCount = baseVm.ReceivedLikesCount,
                Major = baseVm.Major,
                School = baseVm.School,
                FollowRequestAlreadySent = baseVm.FollowRequestAlreadySent,
                IsTargetUserFollowingCurrentUser = baseVm.IsTargetUserFollowingCurrentUser,
                IsCurrentUserFollowingTargetUser = baseVm.IsCurrentUserFollowingTargetUser,
                Photos = photos.Select(p => new PhotoViewModel
                {
                    AlbumId = p.AlbumId,
                    Description = p.Description,
                    FromUserDisplayName = UserService.GetUserDisplayName(p.UserId),
                    FromUserId = p.UserId,
                    FromUserPhotoUrl = PhotoService.GetUserDisplayPhoto(p.UserId),
                    IsFromCurrentUser = p.UserId == currentUserId,
                    PhotoId = p.Id,
                    PhotoUrl = PhotoService.GeneratePhotoUrl(p.UserId, p.Id)
                })
            };

            return View("UserPhotos", vm);
        }

        public ActionResult RenderSingleProfilePhoto(int photoId)
        {
            var photo = PhotoService.GetPhoto(photoId);
            if (photo == null) throw new HttpException(404, "not found");

            var vm = new PhotoViewModel
            {
                AlbumId = photo.AlbumId,
                Description = photo.Description,
                FromUserDisplayName = UserService.GetUserDisplayName(photo.UserId),
                FromUserId = photo.UserId,
                FromUserPhotoUrl = PhotoService.GetUserDisplayPhoto(photo.UserId),
                IsFromCurrentUser = photo.UserId == GetCurrentUserId(),
                PhotoId = photo.Id,
                PhotoUrl = PhotoService.GeneratePhotoUrl(photo.UserId, photo.Id)
            };

            return View("_UserProfileSinglePhoto", vm);
        }

        public ActionResult LoadMoreFeeds(int lastFeedId, bool likedFeed = false)
        {
            var currentUserId = 0;
            if (Request.IsAuthenticated)
                currentUserId = UserService.GetUserId(User.Identity.Name);

            var user = UserService.GetUser(User.Identity.Name);
            
            var feeds = likedFeed
                ? _feedHelper.GetUserLikedFeed(user.Id, currentUserId, lastFeedId)
                : _feedHelper.GetUserActivityFeed(user.Id, currentUserId, lastFeedId);

            return View("_FeedsPartial", feeds);
        }

        [HttpGet, Authorize]
        public ActionResult Edit()
        {
            var user = _uow.UserRepository.GetByUsername(User.Identity.Name, true, true, false, true);
            return user.AccountType == AccountType.User ? EditUser(user) : EditClub(user);
        }

        [HttpGet]
        public ActionResult EditUser(User user)
        {
            EditUserProfileViewModel vm;
            if (user != null)
            {
                vm = new EditUserProfileViewModel
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
            }
            else
            {
                vm = new EditUserProfileViewModel();
            }

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

        [HttpGet]
        public ActionResult EditClub(User user)
        {
            EditClubProfileViewModel vm;

            if (user != null)
            {
                vm = new EditClubProfileViewModel
                {
                    ClubAddress = user.ClubDetail.Address,
                    ClubName = user.ClubDetail.ClubName,
                    ClubType = user.ClubDetail.ClubType,
                    SchoolId = user.ClubDetail.SchoolId,
                    CityId = user.ClubDetail.CityId,
                    SendFbErrorNotification = user.Preferences.SendSyncErrorNotifications,
                    SyncFbEvents = user.Preferences.SyncFbEvents,
                    SyncFbImages = user.Preferences.SyncFbImages,
                    SyncFbPosts = user.Preferences.SyncFbPosts
                };
            }
            else
            {
                vm = new EditClubProfileViewModel();
            }

            vm.Cities = StaticDataRepository.GetCities();
            vm.Schools = StaticDataRepository.GetSchools();

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
                user.ClubDetail.SchoolId = vm.SchoolId;
                user.ClubDetail.CityId = vm.CityId;
                user.Preferences.SendSyncErrorNotifications = vm.SendFbErrorNotification;
                user.Preferences.SyncFbEvents = vm.SyncFbEvents;
                user.Preferences.SyncFbImages = vm.SyncFbImages;
                user.Preferences.SyncFbPosts = vm.SyncFbPosts;

                _uow.SaveChanges();

                _cacheService.RemoveUserDisplayName(user.Id);
                ShowAlert("Your preferences has been updated.", AlertType.Success);
            }

            vm.Cities = StaticDataRepository.GetCities();
            vm.Schools = StaticDataRepository.GetSchools();

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

        [Authorize]
        public ActionResult Followers(int id)
        {
            var user = UserService.GetUser(id, true, true, true, true);
            return user.AccountType == AccountType.User
                ? UserFollowers(user)
                : ClubFollowers(user);
        }

        [Authorize]
        public ActionResult Following(int id)
        {
            var user = UserService.GetUser(id, true, true, true, true);
            return user.AccountType == AccountType.User
                ? UserFollowing(user)
                : ClubFollowing(user);
        }

        [Authorize]
        public ActionResult LikedFeed(int id)
        {
            var user = UserService.GetUser(id, true, true, true, true);
            ViewBag.LikedFeed = true;
            return user.AccountType == AccountType.User
                ? UserLikedFeed(user)
                : ClubLikedFeed(user);
        }

        [Authorize]
        public ActionResult FollowingClubs(int id)
        {
            var user = UserService.GetUser(id, true, false, false, true);
            if (user.AccountType == AccountType.Club)
                throw new HttpException(404, "not found");

            var baseVm = LoadBaseUserProfileVm(user);
            var currentUserId = GetCurrentUserId();

            var clubs = _uow.FollowRepository.GetClubsThatUserFollows(id);
            var clubsVm = new List<ClubViewModel>();
            if (clubs != null)
            {
                var items = clubs.Select(x => new
                {
                    x.Id,
                    ProfileImageId = x.ProfilePhotoId,
                    CoverImageId = x.ClubDetail.CoverPhotoId,
                    IsFollowing = x.Followers.Any(f => f.FromUserId == currentUserId)
                }).ToList();

                clubsVm.AddRange(items.Select(x => new ClubViewModel
                {
                    ClubId = x.Id,
                    ClubName = UserService.GetUserDisplayName(x.Id),
                    ProfileImageLink = x.ProfileImageId.HasValue
                        ? PhotoService.GeneratePhotoUrl(x.Id, x.ProfileImageId.Value)
                        : DefaultImageHelper.GetUserDefaultImage(Gender.NotSpecified),
                    CoverImageLink = x.CoverImageId.HasValue
                        ? PhotoService.GeneratePhotoUrl(x.Id, x.CoverImageId.Value)
                        : DefaultImageHelper.GetDefaultCoverImage(),
                    IsCurrentUserFollowing = x.IsFollowing,
                    CurrentUserId = currentUserId

                }));
            }

            var vm = new UserFollowingClubsViewModel
            {
                CategoriesStats = baseVm.CategoriesStats,
                City = baseVm.City,
                FollowRequestAlreadySent = baseVm.FollowRequestAlreadySent,
                FollowersCount = baseVm.FollowersCount,
                FollowingsCount = baseVm.FollowingsCount,
                IsCurrentUserFollowingTargetUser = baseVm.IsCurrentUserFollowingTargetUser,
                IsSelf = baseVm.IsSelf,
                IsTargetUserFollowingCurrentUser = baseVm.IsTargetUserFollowingCurrentUser,
                Major = baseVm.Major,
                ReceivedLikesCount = baseVm.ReceivedLikesCount,
                School = baseVm.School,
                UserId = baseVm.UserId,
                UserName = baseVm.UserName,
                UserPhoto = baseVm.UserPhoto,
                Clubs = clubsVm
            };

            return View("UserFollowingClubs", vm);
        }

        private ActionResult ClubLikedFeed(User user)
        {
            var baseVm = LoadBaseClubProfileVm(user);
            var currentUserId = GetCurrentUserId();
            var vm = new ClubProfileViewModel
            {
                Address = baseVm.Address,
                ClubType = baseVm.ClubType,
                CoverPhotoUrl = baseVm.CoverPhotoUrl,
                Events = baseVm.Events,
                Feeds = _feedHelper.GetUserLikedFeed(user.Id, currentUserId),
                FollowersCount = baseVm.FollowersCount,
                FollowingsCount = baseVm.FollowingsCount,
                IsCurrentUserFollowingTheClub = baseVm.IsCurrentUserFollowingTheClub,
                IsSelf = baseVm.IsSelf,
                PartyAlbums = baseVm.PartyAlbums,
                ReceivedLikesCount = baseVm.ReceivedLikesCount,
                UserId = baseVm.UserId,
                UserName = baseVm.UserName,
                UserPhoto = baseVm.UserPhoto,
                Weeklies = baseVm.Weeklies
            };

            return View("ClubProfile", vm);
        }

        private ActionResult UserLikedFeed(User user)
        {
            var baseVm = LoadBaseUserProfileVm(user);
            var currentUserId = GetCurrentUserId();
            var vm = new UserProfileViewModel
            {
                CategoriesStats = baseVm.CategoriesStats,
                City = baseVm.City,
                Feeds = _feedHelper.GetUserLikedFeed(user.Id, currentUserId),
                UserId = baseVm.UserId,
                UserName = baseVm.UserName,
                UserPhoto = baseVm.UserPhoto,
                IsSelf = baseVm.IsSelf,
                FollowersCount = baseVm.FollowersCount,
                FollowingsCount = baseVm.FollowingsCount,
                ReceivedLikesCount = baseVm.ReceivedLikesCount,
                Major = baseVm.Major,
                School = baseVm.School,
                FollowRequestAlreadySent = baseVm.FollowRequestAlreadySent,
                IsTargetUserFollowingCurrentUser = baseVm.IsTargetUserFollowingCurrentUser,
                IsCurrentUserFollowingTargetUser = baseVm.IsCurrentUserFollowingTargetUser
            };

            return View("UserProfile", vm);
        }

        private ActionResult UserFollowing(User user)
        {
            var followings = GetFollowings(user.Id);
            var baseVm = LoadBaseUserProfileVm(user);

            var vm = new UserFollowingViewModel
            {
                CategoriesStats = baseVm.CategoriesStats,
                City = baseVm.City,
                FollowRequestAlreadySent = baseVm.FollowRequestAlreadySent,
                FollowersCount = baseVm.FollowersCount,
                FollowingsCount = baseVm.FollowingsCount,
                IsCurrentUserFollowingTargetUser = baseVm.IsCurrentUserFollowingTargetUser,
                IsSelf = baseVm.IsSelf,
                IsTargetUserFollowingCurrentUser = baseVm.IsTargetUserFollowingCurrentUser,
                Major = baseVm.Major,
                ReceivedLikesCount = baseVm.ReceivedLikesCount,
                School = baseVm.School,
                UserId = baseVm.UserId,
                UserName = baseVm.UserName,
                UserPhoto = baseVm.UserPhoto,
                Follows = followings
            };

            return View("UserFollowing", vm);
        }

        private ActionResult ClubFollowing(User user)
        {
            var baseVm = LoadBaseClubProfileVm(user);
            var vm = new ClubFollowingViewModel
            {
                Address = baseVm.Address,
                ClubType = baseVm.ClubType,
                CoverPhotoUrl = baseVm.CoverPhotoUrl,
                Events = baseVm.Events,
                FollowersCount = baseVm.FollowersCount,
                FollowingsCount = baseVm.FollowingsCount,
                IsCurrentUserFollowingTheClub = baseVm.IsCurrentUserFollowingTheClub,
                IsSelf = baseVm.IsSelf,
                PartyAlbums = baseVm.PartyAlbums,
                ReceivedLikesCount = baseVm.ReceivedLikesCount,
                UserId = baseVm.UserId,
                UserName = baseVm.UserName,
                UserPhoto = baseVm.UserPhoto,
                Weeklies = baseVm.Weeklies,
                Follows = GetFollowings(user.Id)
            };

            return View("ClubFollowing", vm);
        }

        private ActionResult UserFollowers(User user)
        {
            var baseVm = LoadBaseUserProfileVm(user);

            IEnumerable<UserViewModel> followRequests = null;
            if (baseVm.IsSelf)
            {
                var requestIds = _followService
                    .GetFollowRequests(user.Id)
                    .Select(f => f.FromUserId).ToList();

                followRequests = requestIds.Select(id => new UserViewModel
                {
                    Id = id,
                    DisplayName = UserService.GetUserDisplayName(id),
                    ProfileImage = PhotoService.GetUserDisplayPhoto(id)
                });
            }

            var vm = new UserFollowersViewModel
            {
                CategoriesStats = baseVm.CategoriesStats,
                City = baseVm.City,
                FollowRequestAlreadySent = baseVm.FollowRequestAlreadySent,
                FollowersCount = baseVm.FollowersCount,
                FollowingsCount = baseVm.FollowingsCount,
                IsCurrentUserFollowingTargetUser = baseVm.IsCurrentUserFollowingTargetUser,
                IsSelf = baseVm.IsSelf,
                IsTargetUserFollowingCurrentUser = baseVm.IsTargetUserFollowingCurrentUser,
                Major = baseVm.Major,
                ReceivedLikesCount = baseVm.ReceivedLikesCount,
                School = baseVm.School,
                UserId = baseVm.UserId,
                UserName = baseVm.UserName,
                UserPhoto = baseVm.UserPhoto,
                Followers = GetFollowers(user.Id),
                FollowRequests = followRequests
            };

            return View("UserFollowers", vm);
        }

        private ActionResult ClubFollowers(User user)
        {
            var baseVm = LoadBaseClubProfileVm(user);
            var vm = new ClubFollowersViewModel
            {
                Address = baseVm.Address,
                ClubType = baseVm.ClubType,
                CoverPhotoUrl = baseVm.CoverPhotoUrl,
                Events = baseVm.Events,
                FollowersCount = baseVm.FollowersCount,
                FollowingsCount = baseVm.FollowingsCount,
                IsCurrentUserFollowingTheClub = baseVm.IsCurrentUserFollowingTheClub,
                IsSelf = baseVm.IsSelf,
                PartyAlbums = baseVm.PartyAlbums,
                ReceivedLikesCount = baseVm.ReceivedLikesCount,
                UserId = baseVm.UserId,
                UserName = baseVm.UserName,
                UserPhoto = baseVm.UserPhoto,
                Weeklies = baseVm.Weeklies,
                Followers = GetFollowers(user.Id)
            };

            return View("ClubFollowers", vm);
        }

        private IEnumerable<UserViewModel> GetFollowings(int userId)
        {
            var follows = _uow.FollowRepository.GetUserFollows(userId)
                .Select(f => new
                {
                    id = f.ToUserId,
                    f.ToUser.ProfilePhotoId
                }).ToList();

            return follows.Select(f => new UserViewModel
            {
                Id = f.id,
                DisplayName = UserService.GetUserDisplayName(f.id),
                ProfileImage = PhotoService.GetUserDisplayPhoto(f.id)
            });
        }

        private IEnumerable<UserViewModel> GetFollowers(int userId)
        {
            var follows = _uow.FollowRepository.GetUserFollowers(userId)
                .Select(f => new
                {
                    id = f.FromUserId,
                    f.FromUser.ProfilePhotoId
                }).ToList();

            return follows.Select(f => new UserViewModel
            {
                Id = f.id,
                DisplayName = UserService.GetUserDisplayName(f.id),
                ProfileImage = PhotoService.GetUserDisplayPhoto(f.id)
            });
        }
    }
}
