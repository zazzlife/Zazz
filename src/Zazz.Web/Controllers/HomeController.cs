using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure;
using Zazz.Web.Helpers;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IUoW _uow;
        private readonly IStaticDataRepository _staticDataRepository;
        private readonly ITagService _tagService;
        private readonly IDefaultImageHelper _defaultImageHelper;

        public HomeController(IUoW uow, IPhotoService photoService, IUserService userService,
            IStaticDataRepository staticDataRepository, ITagService tagService,
            IDefaultImageHelper defaultImageHelper) : base(userService, photoService)
        {
            _uow = uow;
            _staticDataRepository = staticDataRepository;
            _tagService = tagService;
            _defaultImageHelper = defaultImageHelper;
        }

        public ActionResult UpdateTags()
        {
            _tagService.UpdateTagStatistics();
            return Redirect(Request.UrlReferrer.AbsolutePath);
        }

        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = UserService.GetUser(User.Identity.Name);
                var feeds = new FeedHelper(_uow, UserService, _photoService, _defaultImageHelper).GetFeeds(user.Id);

                var tagStats = _tagService.GetAllTagStats().ToList();
                var vm = new UserHomeViewModel
                         {
                             AccountType = user.AccountType,
                             Feeds = feeds,
                             TagStats = new TagStatsWidgetViewModel
                                        {
                                            Tags = tagStats.Select(t => new TagStatViewModel
                                                                        {
                                                                            TagName = t.Tag.Name,
                                                                            UsersCount = t.UsersCount
                                                                        }),
                                            LastUpdate = tagStats.FirstOrDefault() == null
                                                             ? DateTime.MinValue
                                                             : tagStats.First().LastUpdate
                                        }
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
            var availableTags = _staticDataRepository.GetTags().ToList();
            var selectedTags = String.IsNullOrEmpty(@select)
                                   ? Enumerable.Empty<string>()
                                   : @select.Split(',');

            var selectedTagsIds =
                availableTags.Where(t => selectedTags.Contains(t.Name, StringComparer.InvariantCultureIgnoreCase))
                .Select(t => t.Id);

            var feeds = new FeedHelper(_uow, UserService, _photoService, _defaultImageHelper).GetTaggedFeeds(userId,
                                                                                         selectedTagsIds.ToList());

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
            var feeds = new FeedHelper(_uow, UserService, _photoService, _defaultImageHelper)
                                  .GetFeeds(user.Id, lastFeedId);

            return View("_FeedsPartial", feeds);
        }

        public string GetAllTags()
        {
            var tags = _staticDataRepository.GetTags().Select(t => t.Name);
            return JsonConvert.SerializeObject(tags, Formatting.None);
        }

        public JsonNetResult Search(string q)
        {
            var users = _uow.UserRepository.GetAll()
                            .Where(u =>
                                u.UserDetail.FullName.Contains(q) ||
                                u.Username.Contains(q) ||
                                u.ClubDetail.ClubName.Contains(q))
                            .Select(u => new
                                         {
                                             id = u.Id,
                                             username = u.Username,
                                             fullname = u.UserDetail.FullName,
                                             clubname = u.ClubDetail.ClubName
                                         })
                            .Take(5);

            var response = new List<AutocompleteResponse>();

            foreach (var u in users)
            {
                var autocompleteResponse = new AutocompleteResponse
                          {
                              Id = u.id,
                              Img = _photoService.GetUserImageUrl(u.id).VerySmallLink
                          };

                if (!String.IsNullOrEmpty(u.clubname) &&
                    u.clubname.IndexOf(q, StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    autocompleteResponse.Value = u.clubname;
                }
                else if (!String.IsNullOrEmpty(u.fullname) &&
                         u.fullname.IndexOf(q, StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    autocompleteResponse.Value = u.fullname;
                }
                else
                {
                    autocompleteResponse.Value = u.username;
                }

                response.Add(autocompleteResponse);
            }

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
