using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Zazz.Web.Controllers
{
    public class UserController : BaseController
    {
        public ActionResult Index()
        {
            return Me();
        }

        public ActionResult Me()
        {
            return View("Profile");
        }
    }
}
