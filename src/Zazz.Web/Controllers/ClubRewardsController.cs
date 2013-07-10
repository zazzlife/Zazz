using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Zazz.Web.Controllers
{
    [Authorize]
    public class ClubRewardsController : Controller
    {
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            return View();
        }

        public ActionResult New()
        {
            return View();
        }

        public ActionResult Edit()
        {
            return View();
        }

        public ActionResult Remove()
        {
            return View();
        }

        public ActionResult Scenarios()
        {
            return View();
        }

        public ActionResult AddScenario()
        {
            return View();
        }

        public ActionResult RemoveScenario()
        {
            return View();
        }
    }
}
