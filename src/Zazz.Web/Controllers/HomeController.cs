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

        public HomeController(IUoW uow, IPhotoService photoService)
        {
            _uow = uow;
            _photoService = photoService;
        }

        public ActionResult Index()
        {
            using (_uow)
            using (_photoService)
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
                             };

                var fullName = _uow.UserRepository.GetUserFullName(feed.UserId);
                if (String.IsNullOrEmpty(fullName))
                {
                    feedVm.UserDisplayName = _uow.UserRepository.GetUserName(feed.UserId);
                }
                else
                {
                    feedVm.UserDisplayName = fullName;
                }


            }

            throw new NotImplementedException();
        }
    }
}
