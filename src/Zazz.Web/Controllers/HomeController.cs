using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure;
using Zazz.Web.Helpers;
using Zazz.Web.Interfaces;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IFeedHelper _feedHelper;
        private readonly IUoW _uow;

        public HomeController(IPhotoService photoService, IUserService userService,
            IStaticDataRepository staticDataRepository, ICategoryService categoryService,
            IDefaultImageHelper defaultImageHelper, IFeedHelper feedHelper, IUoW uow) :
            base(userService, photoService, defaultImageHelper, staticDataRepository, categoryService)
        {
            _feedHelper = feedHelper;
            _uow = uow;
        }

        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = UserService.GetUser(User.Identity.Name);
                var feeds = _feedHelper.GetFeeds(user.Id);
                
                var vm = new UserHomeViewModel
                         {
                             AccountType = user.AccountType,
                             Feeds = feeds,
                             HasFacebookAccount = UserService.OAuthAccountExists(user.Id, OAuthProvider.Facebook)
                         };

                return View("UserHome", vm);
            }
            else
            {
                return View("LandingPage");
            }
        }

        [Authorize]
        public ActionResult Categories(string @select)
        {
            var user = UserService.GetUser(User.Identity.Name);
            var availableCategories = StaticDataRepository.GetCategories().ToList();
            var selectedCategories = String.IsNullOrEmpty(@select)
                                   ? Enumerable.Empty<string>()
                                   : @select.Split(',');

            var selectedCategoriesId =
                availableCategories.Where(t => selectedCategories.Contains(t.Name,
                                                                           StringComparer.InvariantCultureIgnoreCase))
                                   .Select(t => t.Id);

            var feeds = _feedHelper.GetCategoryFeeds(user.Id, selectedCategoriesId.ToList());

            if (Request.IsAjaxRequest())
                return View("_FeedsPartial", feeds);

            var vm = new CategoriesPageViewModel
                     {
                         AvailableCategories = availableCategories.Select(t => t.Name),
                         SelectedCategories = selectedCategories,
                         Feeds = feeds,
                         AccountType = user.AccountType,
                         HasFacebookAccount = UserService.OAuthAccountExists(user.Id, OAuthProvider.Facebook)
                     };

            return View(vm);
        }

        [Authorize]
        public ActionResult LoadMoreFeeds(int lastFeedId)
        {
            var user = UserService.GetUser(User.Identity.Name);
            var feeds = _feedHelper.GetFeeds(user.Id, lastFeedId);

            return View("_FeedsPartial", feeds);
        }

        [Authorize]
        public ActionResult Clubs(string type)
        {
            var userId = GetCurrentUserId();
            IQueryable<User> clubs = null;

            if (String.IsNullOrEmpty(type) || type.Equals("clubs", StringComparison.InvariantCultureIgnoreCase))
            {
                clubs = _uow.FollowRepository.GetClubsThatUserDoesNotFollow(userId);
            }
            else if (type.Equals("myclubs", StringComparison.InvariantCultureIgnoreCase))
            {
                clubs = _uow.FollowRepository.GetClubsThatUserFollows(userId);
            }
            else if (type.Equals("schoolclubs", StringComparison.InvariantCultureIgnoreCase))
            {
                clubs = _uow.UserRepository.GetSchoolClubs();
            }

            var vm = new List<ClubViewModel>();
            if (clubs != null)
            {
                var items = clubs.Select(x => new
                {
                    x.Id,
                    CoverImageId = x.ClubDetail.CoverPhotoId,
                    IsFollowing = x.Followers.Any(f => f.FromUserId == userId)
                }).ToList();

                vm.AddRange(items.Select(x => new ClubViewModel
                {
                    ClubId = x.Id,
                    ClubName = UserService.GetUserDisplayName(x.Id),
                    CoverImageLink = x.CoverImageId.HasValue
                        ? PhotoService.GeneratePhotoUrl(x.Id, x.CoverImageId.Value)
                        : DefaultImageHelper.GetDefaultCoverImage(),
                    IsCurrentUserFollowing = x.IsFollowing,
                    CurrentUserId = userId

                }));
            }

            return Request.IsAjaxRequest() ? View("_ClubsList", vm) : View(vm);
        }

        public string GetAllCategories()
        {
            var categories = StaticDataRepository.GetCategories().Select(c => c.Name);
            return JsonConvert.SerializeObject(categories, Formatting.None);
        }

        [Authorize]
        public JsonNetResult Search(string q)
        {
            var users = UserService.Search(q);
            var response = users.Select(u => new AutocompleteResponse
                                             {
                                                 Id = u.UserId,
                                                 Value = u.DisplayName,
                                                 Img = u.DisplayPhoto.VerySmallLink
                                             });

            return new JsonNetResult(response);
        }

        public class AutocompleteResponse
        {
            public int Id { get; set; }

            public string Value { get; set; }

            public string Img { get; set; }
        }
    }

    public class LoginSuccessController : Controller
    {
        public string Index()
        {
            return String.Empty;
        }
    }
}
