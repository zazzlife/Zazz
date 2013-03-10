using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class EventController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IPostService _postService;

        public EventController(IUserService userService, IPostService postService)
        {
            _userService = userService;
            _postService = postService;
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult Create()
        {
            ViewBag.FormAction = "Create";
            return View("EditForm");
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize]
        public async Task<ActionResult> Create(EventViewModel vm)
        {
            if (ModelState.IsValid)
            {
                using (_userService)
                using (_postService)
                {
                    var userId = _userService.GetUserId(User.Identity.Name);
                    if (userId == 0)
                        throw new SecurityException();

                    var post = new Post
                                   {
                                       UserId = userId,
                                       Title = vm.Name,
                                       Message = vm.Detail,
                                       IsEvent = true,
                                       CreatedDate = DateTime.UtcNow,
                                       EventDetail = new EventDetail
                                                         {
                                                             City = vm.City,
                                                             Country = vm.Country,
                                                             Location = vm.Location,
                                                             Price = vm.Price,
                                                             StartTime = vm.StartTime,
                                                             Street = vm.Street
                                                         }
                                   };

                    await _postService.CreatePostAsync(post);
                    return Redirect("~/event/show/" + post.Id);
                }
            }

            ViewBag.FormAction = "Create";
            return View("EditForm");
        }

        [HttpGet, Authorize]
        public async Task<ActionResult> Show(int id)
        {
            using (_userService)
            using (_postService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                
                var post = await _postService.GetPostAsync(id);
                if (post == null)
                    throw new HttpException(404, "The requested entry was not found");

                var vm = new EventViewModel
                             {
                                 Id = post.Id,
                                 City = post.EventDetail.City,
                                 Country = post.EventDetail.Country,
                                 CreatedDate = post.CreatedDate,
                                 Detail = post.Message,
                                 Location = post.EventDetail.Location,
                                 Name = post.Title,
                                 Price = post.EventDetail.Price,
                                 StartTime = post.EventDetail.StartTime,
                                 Street = post.EventDetail.Street,
                                 FacebookLink = post.FacebookLink,
                                 IsOwner = post.UserId == userId
                             };

                ViewData.Model = vm;
            }

            return View();
        }
    }
}
