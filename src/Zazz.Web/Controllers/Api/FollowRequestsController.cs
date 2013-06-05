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
    [HMACAuthorize]
    public class FollowRequestsController : BaseApiController
    {
        private readonly IFollowService _followService;
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;

        public FollowRequestsController(IFollowService followService, IUserService userService,
            IPhotoService photoService)
        {
            _followService = followService;
            _userService = userService;
            _photoService = photoService;
        }

        // GET api/v1/followrequests
        public IEnumerable<ApiFollowRequest> Get()
        {
            var userId = ExtractUserIdFromHeader();
            var requests = _followService.GetFollowRequests(userId)
                                         .Select(r => new {fromUserId = r.FromUserId, time = r.RequestDate})
                                         .ToList();

            return requests.Select(r => new ApiFollowRequest
                                        {
                                            UserId = r.fromUserId,
                                            DisplayName = _userService.GetUserDisplayName(r.fromUserId),
                                            DisplayPhoto = _photoService.GetUserImageUrl(r.fromUserId),
                                            Time = r.time
                                        });
        }

        // DELETE api/v1/followrequests/5?action=accept/reject
        public void Delete(int id, string action)
        {
            if (id == 0 || String.IsNullOrWhiteSpace(action))
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            if (action.Equals("accept", StringComparison.InvariantCultureIgnoreCase))
            {
                var userId = ExtractUserIdFromHeader();
                _followService.AcceptFollowRequest(id, userId);
            }
            else if (action.Equals("reject", StringComparison.InvariantCultureIgnoreCase))
            {
                
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
        }
    }
}
