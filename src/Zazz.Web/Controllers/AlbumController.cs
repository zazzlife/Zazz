using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class AlbumController : Controller
    {
        public ActionResult Index()
        {
            return View("List");
        }

        [Authorize, HttpPost]
        public ActionResult CreateAlbum(string albumName)
        {
            return RedirectToAction("Index");
        }
    }
}
