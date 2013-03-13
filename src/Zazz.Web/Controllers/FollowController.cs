using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Zazz.Web.Controllers
{
    [Authorize]
    public class FollowController : BaseController
    {
        public void FollowUser(int id)
        {
            
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
