using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class EventController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IPostService _postService;
        private readonly IUoW _uow;

        private const int PAGE_SIZE = 10;

        public EventController(IUserService userService, IPostService postService, IUoW uow)
        {
            _userService = userService;
            _postService = postService;
            _uow = uow;
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            var vm = new EventListViewModel
                     {
                         MonthEvents = GetMonthEvents(),
                         WeekEvents = GetWeekEvents()
                     };

            return View(vm);
        }

        private IPagedList<EventViewModel> GetWeekEvents(int page = 1)
        {
            var today = DateTime.UtcNow;

            var delta = DayOfWeek.Monday - today.DayOfWeek;
            var firstDayOfWeek = today.AddDays(delta).Date;

            delta = DayOfWeek.Sunday - today.DayOfWeek;
            var lastDayOfWeek = today.AddDays(delta).Date;

            return GetEvents(firstDayOfWeek, lastDayOfWeek, page);
        }

        private IPagedList<EventViewModel> GetMonthEvents(int page = 1)
        {
            var today = DateTime.UtcNow;
            var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);

            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1).Date;
            var lastDayOfMonth = new DateTime(today.Year, today.Month, daysInMonth).Date;

            return GetEvents(firstDayOfMonth, lastDayOfMonth, page);
        }

        private IPagedList<EventViewModel> GetEvents(DateTime from, DateTime to, int page)
        {
            var skip = (page - 1)*PAGE_SIZE;

            var events = _uow.PostRepository.GetEventRange(from, to)
                             .OrderBy(e => e.EventDetail.TimeUtc)
                             .Skip(skip)
                             .Take(PAGE_SIZE)
                             .Select(e => new EventViewModel
                             {
                                 City = e.EventDetail.City,
                                 Id = e.Id,
                                 CreatedDate = e.CreatedDate,
                                 Detail = e.Message,
                                 FacebookLink = e.FacebookPhotoLink,
                                 Latitude = e.EventDetail.Latitude,
                                 Longitude = e.EventDetail.Longitude,
                                 Location = e.EventDetail.Location,
                                 Name = e.Title,
                                 Price = e.EventDetail.Price,
                                 Street = e.EventDetail.Street,
                                 Time = e.EventDetail.Time,
                             });

            var eventsCount = _uow.PostRepository.GetEventRange(from, to).Count();

            return new StaticPagedList<EventViewModel>(events, page, PAGE_SIZE, eventsCount);
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
