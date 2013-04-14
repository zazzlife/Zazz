using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.Helpers;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    [Authorize]
    public class FollowController : BaseController
    {
        private readonly IFollowService _followService;
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;

        public FollowController(IFollowService followService, IUserService userService, IPhotoService photoService)
        {
            _followService = followService;
            _userService = userService;
            _photoService = photoService;
        }

        public void FollowUser(int id)
        {
            var currentUserId = _userService.GetUserId(User.Identity.Name);
            var accountType = _userService.GetUserAccountType(id);

            if (accountType == AccountType.User)
            {
                _followService.SendFollowRequest(currentUserId, id);
            }
            else if (accountType == AccountType.ClubAdmin)
            {
                _followService.FollowClubAdmin(currentUserId, id);
            }
        }

        /// <summary>
        /// This action should be used when current user wants to stop following another user.
        /// </summary>
        /// <param name="id">Id of the other user</param>
        /// <returns></returns>
        public void Unfollow(int id)
        {
            var currentUserId = _userService.GetUserId(User.Identity.Name);
            _followService.RemoveFollow(currentUserId, id);
        }

        /// <summary>
        /// This action should be used when the current user wants to stop another user from following him.
        /// </summary>
        /// <param name="id">Id of the other user</param>
        /// <returns></returns>
        public void StopFollow(int id)
        {
            var currentUserId = _userService.GetUserId(User.Identity.Name);
            _followService.RemoveFollow(id, currentUserId);
        }

        public void AcceptFollow(int id)
        {
            var currentUserId = _userService.GetUserId(User.Identity.Name);
            _followService.AcceptFollowRequest(id, currentUserId);
        }

        public void RejectFollow(int id)
        {
            var currentUserId = _userService.GetUserId(User.Identity.Name);
            _followService.RejectFollowRequest(id, currentUserId);
        }

        public ActionResult GetFollowRequests()
        {
            var userId = _userService.GetUserId(User.Identity.Name);
            var userFollows = _followService.GetFollowRequests(userId);

            var vm = new List<FollowRequestViewModel>();

            foreach (var followRequest in userFollows)
            {
                var r = new FollowRequestViewModel
                        {
                            FromUserId = followRequest.FromUserId,
                            FromUsername = followRequest.FromUser.Username,
                            RequestId = followRequest.Id
                        };

                vm.Add(r);
            }

            return View("_FollowRequestsPartial", vm);
        }

        public string GetFollowRequestsCount()
        {
            var userId = _userService.GetUserId(User.Identity.Name);
            var requestsCount = _followService.GetFollowRequestsCount(userId);

            if (requestsCount == 0)
                return "";
            else
            {
                return
                    "<span id=\"follow-request-count\" class=\"badge badge-small badge-info\">"
                    + requestsCount + "</span>";
            }
        }
    }
}
