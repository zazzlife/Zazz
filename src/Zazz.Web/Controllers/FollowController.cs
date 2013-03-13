using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Web.Controllers
{
    [Authorize]
    public class FollowController : BaseController
    {
        private readonly IFollowService _followService;
        private readonly IUserService _userService;

        public FollowController(IFollowService followService, IUserService userService)
        {
            _followService = followService;
            _userService = userService;
        }

        public async void FollowUser(int id)
        {
            using (_followService)
            using (_userService)
            {
                var currentUserId = _userService.GetUserId(User.Identity.Name);
                var accountType = _userService.GetUserAccountType(id);

                if (accountType == AccountType.User)
                {
                    await _followService.SendFollowRequestAsync(currentUserId, id);
                }
                else if (accountType == AccountType.ClubAdmin)
                {
                    await _followService.FollowClubAdminAsync(currentUserId, id);
                }
            }
        }

        public async void AcceptFollow(int id)
        {
            using (_followService)
            using (_userService)
            {
                var currentUserId = _userService.GetUserId(User.Identity.Name);
                await _followService.AcceptFollowRequestAsync(id, currentUserId);
            }
        }

        public async void RejectFollow(int id)
        {
            using (_followService)
            using (_userService)
            {
                var currentUserId = _userService.GetUserId(User.Identity.Name);
                await _followService.RejectFollowRequestAsync(id, currentUserId);
            }
        }

        public ActionResult GetFollowRequests()
        {
            return View("_FollowRequestsPartial");
        }
    }
}
