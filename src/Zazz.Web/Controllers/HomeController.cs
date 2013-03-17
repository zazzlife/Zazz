using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
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
            using (_uow)
            using (_photoService)
            using (_userService)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var feeds = GetFeeds();
                    return View("UserHome", feeds);
                }
                else
                {
                    return View("LandingPage");
                }
            }
        }

        private List<FeedViewModel> GetFeeds(int page = 0)
        {
            const int PAGE_SIZE = 10;
            var skip = PAGE_SIZE*page;

            var userId = _uow.UserRepository.GetIdByUsername(User.Identity.Name);
            var followIds = _uow.FollowRepository.GetFollowsUserIds(userId);
            var feeds = _uow.FeedRepository.GetFeeds(followIds)
                            .OrderByDescending(f => f.Time)
                            .Skip(skip)
                            .Take(PAGE_SIZE);

            var vm = new List<FeedViewModel>();

            foreach (var feed in feeds)
            {
                var feedVm = new FeedViewModel
                             {
                                 UserId = feed.UserId,
                                 UserDisplayName = _userService.GetUserDisplayName(feed.UserId),
                                 UserImageUrl = _photoService.GetUserImageUrl(feed.UserId),
                                 Time = feed.Time.ToRelativeTime()
                             };

                if (feed.FeedType == FeedType.Event)
                {
                    // EVENT
                    feedVm.EventViewModel = new EventViewModel
                                            {
                                                City = feed.Post.EventDetail.City,
                                                CreatedDate = feed.Post.CreatedDate,
                                                Detail = feed.Post.Message,
                                                FacebookLink = feed.Post.FacebookLink,
                                                Id = feed.Post.Id,
                                                IsOwner = false,
                                                Latitude = feed.Post.EventDetail.Latitude,
                                                Location = feed.Post.EventDetail.Location,
                                                Longitude = feed.Post.EventDetail.Longitude,
                                                Name = feed.Post.Title,
                                                Price = feed.Post.EventDetail.Price,
                                                StartTime = feed.Post.EventDetail.StartTime,
                                                Street = feed.Post.EventDetail.Street
                                            };
                }
                else if (feed.FeedType == FeedType.Picture)
                {
                    var photo = _uow.PhotoRepository.GetPhotoWithMinimalData(feed.PhotoId.Value);
                    feedVm.PhotoUrl = _photoService.GeneratePhotoUrl(photo.UploaderId, photo.AlbumId, photo.Id);
                }

                vm.Add(feedVm);
            }

            return vm;
        }
    }
}
