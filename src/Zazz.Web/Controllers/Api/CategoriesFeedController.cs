using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Web.Filters;
using Zazz.Web.Helpers;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [OAuth2Authorize]
    public class CategoriesFeedController : BaseApiController
    {
        private readonly IFeedHelper _feedHelper;
        private readonly IStaticDataRepository _staticDataRepository;
        private readonly IObjectMapper _objectMapper;

        public CategoriesFeedController(IFeedHelper feedHelper, IStaticDataRepository staticDataRepository,
            IObjectMapper objectMapper)
        {
            _feedHelper = feedHelper;
            _staticDataRepository = staticDataRepository;
            _objectMapper = objectMapper;
        }

        // GET api/Categories/1,2,3,4,5/feed?lastFeed=
        public IEnumerable<ApiFeed> Get(string id, int lastFeed = 0)
        {
            var currentUserId = CurrentUserId;

            if (String.IsNullOrEmpty(id))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var categories = id.Split(',');
            var categoryIds = new List<byte>();

            foreach (var c in categories)
            {
                byte b;
                if (Byte.TryParse(c, out b))
                    categoryIds.Add(b);
            }

            if (categoryIds.Count < 1)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var availableCategories = _staticDataRepository.GetCategories().Select(t => t.Id);
            var validRequestedCategories = categoryIds.Intersect(availableCategories).ToList();

            if (validRequestedCategories.Count < 1)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return _feedHelper.GetCategoryFeeds(currentUserId, validRequestedCategories, lastFeed).feeds
                             .Select(_objectMapper.FeedViewModelToApiModel);

        }
    }
}
