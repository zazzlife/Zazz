using System;
using System.Collections.Generic;
using System.Globalization;
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
                        throw new HttpException(401, "Unauthorized");

                    var post = EventViewModelToPost(vm, userId);
                    post.CreatedDate = DateTime.UtcNow;

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
            var vm = await GetEventAsync(id, false);
            return View(vm);
        }

        [HttpGet, Authorize]
        public async Task<ActionResult> Edit(int id)
        {
            var vm = await GetEventAsync(id, true);
            ViewBag.FormAction = "Edit";

            return View("EditForm", vm);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, EventViewModel vm)
        {
            if (ModelState.IsValid)
            {
                using (_userService)
                using (_postService)
                {
                    var userId = _userService.GetUserId(User.Identity.Name);
                    var post = EventViewModelToPost(vm, userId);
                    post.Id = id;
                    post.EventDetail.Id = id;
                    post.CreatedDate = vm.CreatedDate.Value;

                    await _postService.UpdatePostAsync(post, userId);
                }

                return Redirect("~/event/show/" + id);
            }

            return View("EditForm", vm);
        }

        [Authorize]
        public async Task<ActionResult> Remove(int id)
        {
            using (_userService)
            using (_postService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                await _postService.DeletePostAsync(id, userId);
            }

            ShowAlert("The event has been deleted.", AlertType.Success);
            return Redirect("~/");
        }

        private static Post EventViewModelToPost(EventViewModel vm, int userId)
        {
            var post = new Post
                           {
                               Title = vm.Name,
                               Message = vm.Detail,
                               IsEvent = true,
                               UserId = userId,
                               EventDetail = new EventDetail
                                                 {
                                                     City = vm.City,
                                                     Location = vm.Location,
                                                     Price = vm.Price,
                                                     Time = vm.Time,
                                                     Street = vm.Street,
                                                     Latitude = vm.Latitude,
                                                     Longitude = vm.Longitude
                                                 }
                           };

            post.EventDetail.TimeUtc = DateTime.Parse(vm.UtcTime, CultureInfo.InvariantCulture,
                                                      DateTimeStyles.RoundtripKind);

            return post;
        }

        public async Task<EventViewModel> GetEventAsync(int id, bool ownerOnly)
        {
            using (_userService)
            using (_postService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);

                var post = await _postService.GetPostAsync(id);
                if (post == null)
                    throw new HttpException(404, "The requested entry was not found");

                if (ownerOnly && userId != post.UserId)
                    throw new HttpException(401, "Unauthorized");

                var vm = new EventViewModel
                {
                    Id = post.Id,
                    City = post.EventDetail.City,
                    CreatedDate = post.CreatedDate,
                    Detail = post.Message,
                    Location = post.EventDetail.Location,
                    Name = post.Title,
                    Price = post.EventDetail.Price,
                    Time = post.EventDetail.Time,
                    Street = post.EventDetail.Street,
                    FacebookLink = post.FacebookLink,
                    IsOwner = post.UserId == userId,
                    Latitude = post.EventDetail.Latitude,
                    Longitude = post.EventDetail.Longitude
                };

                return vm;
            }
        }
    }
}
