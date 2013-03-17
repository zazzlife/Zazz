using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
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
            using (_uow)
            using (_photoService)
            using (_userService)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var feeds = GetFeeds(0);
                    return View("UserHome", feeds);
                }
                else
                {
                    return View("LandingPage");
                }
            }
        }

        private List<FeedViewModel> GetFeeds(int page)
        {
            var userId = _uow.UserRepository.GetIdByUsername(User.Identity.Name);
            var followIds = _uow.FollowRepository.GetFollowsUserIds(userId);
            var feeds = _uow.FeedRepository.GetFeeds(followIds);

            var vm = new List<FeedViewModel>();

            foreach (var feed in feeds)
            {
                var feedVm = new FeedViewModel
                             {
                                 UserId = feed.UserId,
                                 UserDisplayName = _userService.GetUserDisplayName(feed.UserId),
                                 UserImageUrl = _photoService.GetUserImageUrl(feed.UserId),
                             };

                


            }

            throw new NotImplementedException();
        }
    }
}
