using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Helpers;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    [Authorize]
    public class FollowController : BaseController
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService, IUserService userService,
                                IPhotoService photoService, IDefaultImageHelper defaultImageHelper,
                                IStaticDataRepository staticDataRepository, ICategoryService categoryService)
            : base(userService, photoService, defaultImageHelper, staticDataRepository, categoryService)
        {
            _followService = followService;
        }

        public void FollowUser(int id)
        {
            var currentUserId = UserService.GetUserId(User.Identity.Name);
            _followService.Follow(currentUserId, id);
        }

        /// <summary>
        /// This action should be used when current user wants to stop following another user.
        /// </summary>
        /// <param name="id">Id of the other user</param>
        /// <returns></returns>
        public void Unfollow(int id)
        {
            var currentUserId = UserService.GetUserId(User.Identity.Name);
            _followService.RemoveFollow(currentUserId, id);
        }

        /// <summary>
        /// This action should be used when the current user wants to stop another user from following him.
        /// </summary>
        /// <param name="id">Id of the other user</param>
        /// <returns></returns>
        public void StopFollow(int id)
        {
            var currentUserId = UserService.GetUserId(User.Identity.Name);
            _followService.RemoveFollow(id, currentUserId);
        }

        public void AcceptFollow(int id)
        {
            var currentUserId = UserService.GetUserId(User.Identity.Name);
            _followService.AcceptFollowRequest(id, currentUserId);
        }

        public void RejectFollow(int id)
        {
            var currentUserId = UserService.GetUserId(User.Identity.Name);
            _followService.RejectFollowRequest(id, currentUserId);
        }

        public ActionResult GetFollowRequests()
        {
            var userId = UserService.GetUserId(User.Identity.Name);
            var userFollows = _followService.GetFollowRequests(userId);

            var vm = userFollows.Select(f => new FollowRequestViewModel
                                             {
                                                 FromUserId = f.FromUserId,
                                                 FromUsername = f.FromUser.Username,
                                             });

            return View("_FollowRequestsPartial", vm);
        }

        public string GetFollowRequestsCount()
        {
            var userId = UserService.GetUserId(User.Identity.Name);
            var requestsCount = _followService.GetFollowRequestsCount(userId);

            if (requestsCount == 0)
                return "";
            else
            {
                return
                    "<span style=\"margin-left: 3px;\" id=\"follow-request-count\" class=\"badge badge-small badge-info\">"
                    + requestsCount + "</span>";
            }
        }
    }
}
