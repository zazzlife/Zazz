using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Web.Filters;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    //[HMACAuthorize]
    public class FollowsController : BaseApiController
    {
        private readonly IFollowService _followService;
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;

        public FollowsController(IFollowService followService, IUserService userService,
            IPhotoService photoService)
        {
            _followService = followService;
            _userService = userService;
            _photoService = photoService;
        }

        // GET api/v1/follows
        public IEnumerable<ApiFollow> Get()
        {
            var userId = ExtractUserIdFromHeader();
            var followers = _followService.GetFollowers(userId)
                .Select(f => f.FromUserId);

            return followers.Select(x => new ApiFollow
                                         {
                                             UserId = x,
                                             DisplayName = _userService.GetUserDisplayName(x),
                                             DisplayPhoto = _photoService.GetUserImageUrl(x)
                                         });
        }

        // POST api/v1/follows
        public void Post([FromBody]int userId)
        {
            if (userId == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var currentUserId = ExtractUserIdFromHeader();
            _followService.Follow(currentUserId, userId);
        }

        // DELETE api/v1/follows/5
        public void Delete(int id)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            var currentUserId = ExtractUserIdFromHeader();
            _followService.RemoveFollow(currentUserId, id);
        }
    }
}
