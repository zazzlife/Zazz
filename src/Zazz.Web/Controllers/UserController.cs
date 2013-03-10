using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Web.Models;

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

        [HttpGet, Authorize]
        public ActionResult Edit()
        {
            return View();
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult Edit(EditProfileViewModel vm)
        {
            return View();
        }
    }
}
