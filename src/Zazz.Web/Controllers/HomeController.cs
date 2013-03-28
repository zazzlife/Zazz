using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Infrastructure;
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

        public async Task<ActionResult> Index()
        {
            using (_uow)
            using (_photoService)
            using (_userService)
            {
                if (User.Identity.IsAuthenticated)
                {
                    var user = await _userService.GetUserAsync(User.Identity.Name);
                    var feeds = await  new FeedHelper(_uow, _userService, _photoService).GetFeedsAsync(user.Id);
                    

                    var vm = new UserHomeViewModel
                             {
                                 AccountType = user.AccountType,
                                 Feeds = feeds
                             };

                    return View("UserHome", vm);
                }
                else
                {
                    return View("LandingPage");
                }
            }
        }

        public JsonNetResult Search(string q)
        {
            using (_uow)
            using (_photoService)
            using (_userService)
            {
                var users = _uow.UserRepository.GetAll()
                                .Where(u =>
                                    u.UserDetail.FullName.Contains(q) || u.Username.Contains(q) ||
                                    u.ClubDetail.ClubName.Contains(q))
                                .Select(u => new
                                             {
                                                 id = u.Id,
                                                 username = u.Username,
                                                 fullname = u.UserDetail.FullName,
                                                 gender = u.UserDetail.Gender,
                                                 picId = u.UserDetail.ProfilePhotoId
                                             })
                                .Take(5);

                var response = new List<AutocompleteResponse>();

                foreach (var u in users)
                {
                    response.Add(new AutocompleteResponse
                                 {
                                     Id = u.id,
                                     Value = !String.IsNullOrEmpty(u.fullname)
                                                 ? u.fullname
                                                 : u.username,
                                     Img = u.picId == 0
                                               ? DefaultImageHelper.GetUserDefaultImage(u.gender)
                                               : _photoService.GeneratePhotoUrl(u.id, u.picId)
                                 });
                }


                return new JsonNetResult {Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
            }
        }

        public class AutocompleteResponse
        {
            public int Id { get; set; }

            public string Value { get; set; }

            public string Img { get; set; }
        }
    }
}
