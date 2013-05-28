using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Web.Filters;
using Zazz.Web.Helpers;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class TaggedFeedsController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        private readonly IUoW _uow;
        private readonly IDefaultImageHelper _defaultImageHelper;
        private readonly IStaticDataRepository _staticDataRepository;

        public TaggedFeedsController(IUserService userService, IPhotoService photoService, IUoW uow,
            IDefaultImageHelper defaultImageHelper, IStaticDataRepository staticDataRepository)
        {
            _userService = userService;
            _photoService = photoService;
            _uow = uow;
            _defaultImageHelper = defaultImageHelper;
            _staticDataRepository = staticDataRepository;
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

            var feedHelper = new FeedHelper(_uow, _userService, _photoService, _defaultImageHelper);

            return feedHelper.GetTaggedFeeds(currentUserId, validRequestedTags, lastFeed)
                             .Select(feedHelper.FeedViewModelToApiModel);

        }
    }
}
