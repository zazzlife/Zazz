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
using Zazz.Web.Interfaces;
using Zazz.Web.Models;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [OAuth2Authorize]
    public class FeedsController : BaseApiController
    {
        private readonly IFeedHelper _feedHelper;
        private readonly IObjectMapper _objectMapper;

        public FeedsController(IFeedHelper feedHelper, IObjectMapper objectMapper)
        {
            _feedHelper = feedHelper;
            _objectMapper = objectMapper;
        }

        // GET api/v1/feeds?lastFeed=
        public IEnumerable<ApiFeed> GetHomeFeeds(int lastFeed = 0)
        {
            var userId = CurrentUserId;
            var feeds = _feedHelper.GetFeeds(userId, lastFeed);

            return feeds.Select(_objectMapper.FeedViewModelToApiModel);
        }

        // GET api/v1/feeds?id=&lastFeed=
        public IEnumerable<ApiFeed> GetUserFeeds(int id, int lastFeed = 0)
        {
            var userId = CurrentUserId;
            var feeds = _feedHelper.GetUserActivityFeed(id, userId, lastFeed);

            return feeds.Select(_objectMapper.FeedViewModelToApiModel);
        }

        
    }
}
