﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Zazz.Web.Controllers
{
    public class AlbumController : Controller
    {
        public ActionResult Index()
        {
            return View("List");
        }
    }
}
