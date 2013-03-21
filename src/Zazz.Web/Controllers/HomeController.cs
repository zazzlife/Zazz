using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.Helpers;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IUoW _uow;
        private readonly IPhotoService _photoService;
        private readonly IUserService _userService;

        public HomeController(IUoW uow, IPhotoService photoService, IUserService userService)
        {
            _uow = uow;
            _photoService = photoService;
            _userService = userService;
        }

        public ActionResult Index()
        {
            using (_uow)
            using (_photoService)
            using (_userService)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = _userService.GetUserId(User.Identity.Name);
                    var feeds = new FeedHelper(_uow, _userService, _photoService).GetFeeds(userId);
                    return View("UserHome", feeds);
                }
                else
                {
                    return View("LandingPage");
                }
            }
        }

        public JsonNetResult Search(string q)
        {
            var response = new List<AutocompleteResponse>();
            for (int i = 0; i < 5; i++)
            {
                response.Add(new AutocompleteResponse
                             {
                                 Id = i,
                                 Img = "/picture/user/1/8/38.jpg",
                                 Value = "User " + i
                             });
            }

            return new JsonNetResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public class AutocompleteResponse
        {
            public int Id { get; set; }

            public string Value { get; set; }

            public string Img { get; set; }
        }
    }
}
