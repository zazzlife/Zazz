using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Models.Data;

namespace Zazz.Web.Controllers
{
    public class CommentController : Controller
    {
        public ActionResult New(int id, FeedType feedType, string comment)
        {
            return View("FeedItems/_SingleComment");
        }
    }
}
