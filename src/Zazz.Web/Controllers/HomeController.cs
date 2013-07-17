using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure;
using Zazz.Web.Helpers;
using Zazz.Web.Interfaces;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class HomeController : UserPageLayoutBaseController
    {
        private readonly IStaticDataRepository _staticDataRepository;
        private readonly IFeedHelper _feedHelper;

        public HomeController(IPhotoService photoService, IUserService userService,
            IStaticDataRepository staticDataRepository, ICategoryService categoryService,
            IDefaultImageHelper defaultImageHelper, IFeedHelper feedHelper) :
            base(userService, photoService, defaultImageHelper, categoryService)
        {
            _staticDataRepository = staticDataRepository;
            _feedHelper = feedHelper;
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
        public ActionResult Tags(string @select)
        {
            var userId = UserService.GetUserId(User.Identity.Name);
            var availableTags = _staticDataRepository.GetCategories().ToList();
            var selectedTags = String.IsNullOrEmpty(@select)
                                   ? Enumerable.Empty<string>()
                                   : @select.Split(',');

            var selectedTagsIds =
                availableTags.Where(t => selectedTags.Contains(t.Name, StringComparer.InvariantCultureIgnoreCase))
                .Select(t => t.Id);

            var feeds = _feedHelper.GetTaggedFeeds(userId, selectedTagsIds.ToList());

            if (Request.IsAjaxRequest())
                return View("_FeedsPartial", feeds);

            var vm = new TagsPageViewModel
                     {
                         AvailableTags = availableTags.Select(t => t.Name),
                         SelectedTags = selectedTags,
                         Feeds = feeds
                     };

            return View(vm);
        }

        public ActionResult LoadMoreFeeds(int lastFeedId)
        {

            var user = UserService.GetUser(User.Identity.Name);
            var feeds = _feedHelper.GetFeeds(user.Id, lastFeedId);

            return View("_FeedsPartial", feeds);
        }

        public string GetAllTags()
        {
            var tags = _staticDataRepository.GetCategories().Select(t => t.Name);
            return JsonConvert.SerializeObject(tags, Formatting.None);
        }

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
}
