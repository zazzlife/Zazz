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
using Zazz.Core.Models.Data;
using System.Collections.ObjectModel;

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

        // TODO: rename
        public IEnumerable<CategoryStatViewModel> GetTagStats(int? currentUserId = null, bool userFollow = false, bool inSameCity = false)
        {
            if ((userFollow || inSameCity) && currentUserId != null){
                User currentUser = UserService.GetUser(currentUserId.GetValueOrDefault());

                ICollection<CategoryStatViewModel> tagStats = new Collection<CategoryStatViewModel>();

                IEnumerable<CategoryStat> stats = CategoryService.GetAllStats().ToList();
                

                foreach (CategoryStat stat in CategoryService.GetAllStats())
                {
                    int usersCount = 0;
                    foreach (StatUser statuser in stat.StatUsers)
                    {
                        User user = UserService.GetUser(currentUserId.GetValueOrDefault(), true, true, false, false);

                        foreach (Follow follow in currentUser.Follows)
                        {
                            User followUser = UserService.GetUser(follow.ToUserId, true, true, false, false);

                            if ((userFollow && inSameCity && follow.ToUserId == statuser.UserId
                                && followUser.UserDetail.CityId == user.UserDetail.CityId)
                                || (userFollow && follow.ToUserId == statuser.UserId)
                                || (inSameCity && followUser.UserDetail.CityId == user.UserDetail.CityId))
                            {
                                usersCount++;
                                break;
                            }
                        }
                    }
                    tagStats.Add(new CategoryStatViewModel
                            {
                                CategoryName = stat.Category.Name,
                                UsersCount = usersCount
                            });
                }
                return tagStats;
            }else{
                return CategoryService.GetAllStats()
                                .Select(t => new CategoryStatViewModel
                                {
                                    CategoryName = t.Category.Name,
                                    UsersCount = t.UsersCount
                                });
            }
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

        public IEnumerable<ClubType> GetClubTypes()
        {
            return (IEnumerable<ClubType>)Enum.GetValues(typeof(ClubType));
        }


        public IEnumerable<User> getUsers()
        {
            var user = UserService.GetUser(User.Identity.Name);

            var follows = user.Follows;

            List<User> users = new List<User>();

            foreach (var follow in follows)
            {
                var usr = UserService.GetUser(follow.ToUserId);

                if (usr.AccountType == AccountType.User)
                    users.Add(usr);
            }
            return users;
        }

        public IEnumerable<User> getAllClubs()
        {
            var user = UserService.GetUser(User.Identity.Name);

            var follows = user.Follows;

            List<User> users = new List<User>();

            foreach (var follow in follows)
            {
                var usr = UserService.GetUser(follow.ToUserId);

                if (usr.AccountType == AccountType.Club)
                    users.Add(usr);
            }
            return users;
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