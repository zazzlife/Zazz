using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;

namespace Zazz.Web.Controllers
{
    [Authorize]
    public class FollowController : BaseController
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        public void FollowUser(int id)
        {
            using (_followService)
            {
                
            }
        }

        public void AcceptFollow(int id)
        {
            
        }

        public void RejectFollow(int id)
        {
            
        }

        public ActionResult GetFollowRequests()
        {
            return View("_FollowRequestsPartial");
        }
    }
}
