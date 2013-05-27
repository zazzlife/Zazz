using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Filters;
using Zazz.Web.Helpers;
using Zazz.Web.Models;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class FeedsController : BaseApiController
    {
        private readonly IUoW _uow;
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        private readonly IDefaultImageHelper _defaultImageHelper;

        public FeedsController(IUoW uow, IUserService userService, IPhotoService photoService,
            IDefaultImageHelper defaultImageHelper)
        {
            _uow = uow;
            _userService = userService;
            _photoService = photoService;
            _defaultImageHelper = defaultImageHelper;
        }

        // GET api/v1/feeds
        public IEnumerable<ApiFeed> GetHomeFeeds()
        {
            var userId = ExtractUserIdFromHeader();
            var feedHelper = new FeedHelper(_uow, _userService, _photoService, _defaultImageHelper);
            var feeds = feedHelper.GetFeeds(userId);

            return feeds.Select(feedHelper.FeedViewModelToApiModel);
        }

        // GET api/v1/feeds?lastFeed=
        public IEnumerable<object> GetHomeFeeds(int lastFeed)
        {
            var userId = ExtractUserIdFromHeader();
            var feedHelper = new FeedHelper(_uow, _userService, _photoService, _defaultImageHelper);
            var feeds = feedHelper.GetFeeds(userId, lastFeed);

            return feeds.Select(feedHelper.FeedViewModelToApiModel);
        }

        // GET api/v1/feeds?id=
        public IEnumerable<object> GetUserFeeds(int id)
        {
            var userId = ExtractUserIdFromHeader();
            var feedHelper = new FeedHelper(_uow, _userService, _photoService, _defaultImageHelper);
            var feeds = feedHelper.GetUserActivityFeed(id, userId);

            return feeds.Select(feedHelper.FeedViewModelToApiModel);
        }

        // GET api/v1/feeds?id=&lastFeed=
        public IEnumerable<object> GetUserFeeds(int id, int lastFeed)
        {
            var userId = ExtractUserIdFromHeader();
            var feedHelper = new FeedHelper(_uow, _userService, _photoService, _defaultImageHelper);
            var feeds = feedHelper.GetUserActivityFeed(id, userId, lastFeed);

            return feeds.Select(feedHelper.FeedViewModelToApiModel);
        }

        
    }
}
