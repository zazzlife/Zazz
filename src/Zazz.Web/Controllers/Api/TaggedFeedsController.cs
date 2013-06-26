using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Web.Filters;
using Zazz.Web.Helpers;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    //[HMACAuthorize]
    public class TaggedFeedsController : BaseApiController
    {
        private readonly IFeedHelper _feedHelper;
        private readonly IStaticDataRepository _staticDataRepository;
        private readonly IObjectMapper _objectMapper;

        public TaggedFeedsController(IFeedHelper feedHelper, IStaticDataRepository staticDataRepository,
            IObjectMapper objectMapper)
        {
            _feedHelper = feedHelper;
            _staticDataRepository = staticDataRepository;
            _objectMapper = objectMapper;
        }

        // GET api/tagfeeds/1,2,3,4,5
        public IEnumerable<ApiFeed> Get(string id, int lastFeed = 0)
        {
            var currentUserId = ExtractUserIdFromHeader();

            if (String.IsNullOrEmpty(id))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var tags = id.Split(',');
            var tagIds = new List<byte>();

            foreach (var tag in tags)
            {
                byte b;
                if (Byte.TryParse(tag, out b))
                    tagIds.Add(b);
            }

            if (tagIds.Count < 1)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var availableTags = _staticDataRepository.GetTags().Select(t => t.Id);
            var validRequestedTags = tagIds.Intersect(availableTags).ToList();

            if (validRequestedTags.Count < 1)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return _feedHelper.GetTaggedFeeds(currentUserId, validRequestedTags, lastFeed)
                             .Select(_objectMapper.FeedViewModelToApiModel);

        }
    }
}
