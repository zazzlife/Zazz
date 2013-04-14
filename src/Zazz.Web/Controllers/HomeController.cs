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
    public class HomeController : BaseController
    {
        private readonly IUoW _uow;
        private readonly IPhotoService _photoService;
        private readonly IUserService _userService;

        public HomeController(IUoW uow, IPhotoService photoService, IUserService userService)
        {
            _uow = uow;
            _photoService = photoService;
            _userService = userService;
        }

        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = _userService.GetUser(User.Identity.Name);
                var feeds = new FeedHelper(_uow, _userService, _photoService).GetFeeds(user.Id);

                var vm = new UserHomeViewModel
                         {
                             AccountType = user.AccountType,
                             Feeds = feeds,
                             CurrentUserDisplayName = _userService.GetUserDisplayName(user.Id),
                             CurrentUserPhoto = _photoService.GetUserImageUrl(user.Id)
                         };

                return View("UserHome", vm);
            }
            else
            {
                return View("LandingPage");
            }
        }

        public ActionResult LoadMoreFeeds(int lastFeedId)
        {

            var user = _userService.GetUser(User.Identity.Name);
            var feeds = new FeedHelper(_uow, _userService, _photoService)
                                  .GetFeeds(user.Id, lastFeedId);

            return View("_FeedsPartial", feeds);
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
