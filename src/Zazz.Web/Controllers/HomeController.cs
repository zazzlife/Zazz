using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;

namespace Zazz.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IUoW _uow;

        public HomeController(IUoW uow)
        {
            _uow = uow;
        }

        public ActionResult Index()
        {
            using (_uow)
            {
                if (User.Identity.IsAuthenticated)
                {
                    

                    return View("UserHome");
                }
                else
                {
                    return View("LandingPage");
                }
            }
        }
    }
}
