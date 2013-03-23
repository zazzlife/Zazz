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
        private readonly IEventService _eventService;
        private readonly IUoW _uow;
        private readonly IPhotoService _photoService;

        private const int PAGE_SIZE = 10;

        public EventController(IUserService userService, IEventService eventService, IUoW uow, IPhotoService photoService)
        {
            _userService = userService;
            _eventService = eventService;
            _uow = uow;
            _photoService = photoService;
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

            var delta = DayOfWeek.Sunday - today.DayOfWeek;
            var firstDayOfWeek = today.AddDays(delta).Date;

            delta = DayOfWeek.Saturday - today.DayOfWeek;
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
            var skip = (page - 1) * PAGE_SIZE;

            var events = _uow.EventRepository.GetEventRange(from, to)
                             .OrderBy(e => e.TimeUtc)
                             .Skip(skip)
                             .Take(PAGE_SIZE)
                             .Select(e => new EventViewModel
                             {
                                 City = e.City,
                                 Id = e.Id,
                                 CreatedDate = e.CreatedDate,
                                 Description = e.Description,
                                 FacebookLink = e.FacebookPhotoLink,
                                 Latitude = e.Latitude,
                                 Longitude = e.Longitude,
                                 Location = e.Location,
                                 Name = e.Name,
                                 Price = e.Price,
                                 Street = e.Street,
                                 Time = e.Time,
                                 PhotoId = e.PhotoId
                             }).ToList();

            foreach (var e in events)
            {
                if (!e.PhotoId.HasValue)
                    continue;

                var photo = _uow.PhotoRepository.GetPhotoWithMinimalData(e.PhotoId.Value);
                if (photo == null)
                    continue;

                e.ImageUrl = _photoService.GeneratePhotoUrl(photo.UploaderId, photo.Id);
            }

            var eventsCount = _uow.EventRepository.GetEventRange(from, to).Count();

            return new StaticPagedList<EventViewModel>(events, page, PAGE_SIZE, eventsCount);
        }

        [HttpGet, Authorize]
        public ActionResult Create()
        {
            using (_userService)
            using (_eventService)
            using (_uow)
            using (_photoService)
            {
                ViewBag.FormAction = "Create";
                return View("EditForm", new EventViewModel
                                        {
                                            Time = DateTime.UtcNow,
                                            UtcTime = DateTime.UtcNow.ToString("s")
                                        });
            }
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize]
        public async Task<ActionResult> Create(EventViewModel vm)
        {
            using (_userService)
            using (_eventService)
            using (_uow)
            using (_photoService)
            {
                if (ModelState.IsValid)
                {

                    var userId = _userService.GetUserId(User.Identity.Name);
                    if (userId == 0)
                        throw new HttpException(401, "Unauthorized");

                    var post = EventViewModelToPost(vm, userId);
                    post.CreatedDate = DateTime.UtcNow;

                    await _eventService.CreateEventAsync(post);
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
                using (_eventService)
                using (_uow)
                using (_photoService)
                {
                    var userId = _userService.GetUserId(User.Identity.Name);
                    var post = EventViewModelToPost(vm, userId);
                    post.Id = id;
                    post.CreatedDate = vm.CreatedDate.Value;

                    await _eventService.UpdateEventAsync(post, userId);
                }

                return Redirect("~/event/show/" + id);
            }

            return View("EditForm", vm);
        }

        [Authorize]
        public async Task<ActionResult> Remove(int id)
        {
            using (_userService)
            using (_eventService)
            using (_uow)
            using (_photoService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                await _eventService.DeleteEventAsync(id, userId);
            }

            ShowAlert("The event has been deleted.", AlertType.Success);
            return Redirect("~/");
        }

        private static ZazzEvent EventViewModelToPost(EventViewModel vm, int userId)
        {
            var post = new ZazzEvent
                       {
                           Name = vm.Name,
                           Description = vm.Description,
                           UserId = userId,
                           PhotoId = vm.PhotoId,
                           City = vm.City,
                           Location = vm.Location,
                           Price = vm.Price,
                           Time = vm.Time,
                           Street = vm.Street,
                           Latitude = vm.Latitude,
                           Longitude = vm.Longitude
                       };

            post.TimeUtc = DateTime.Parse(vm.UtcTime, CultureInfo.InvariantCulture,
                                                      DateTimeStyles.RoundtripKind);

            return post;
        }

        public async Task<EventViewModel> GetEventAsync(int id, bool ownerOnly)
        {
            using (_userService)
            using (_eventService)
            using (_uow)
            using (_photoService)
            {
                var userId = _userService.GetUserId(User.Identity.Name);

                var post = await _eventService.GetEventAsync(id);
                if (post == null)
                    throw new HttpException(404, "The requested entry was not found");

                if (ownerOnly && userId != post.UserId)
                    throw new HttpException(401, "Unauthorized");

                var vm = new EventViewModel
                {
                    Id = post.Id,
                    City = post.City,
                    CreatedDate = post.CreatedDate,
                    Description = post.Description,
                    Location = post.Location,
                    Name = post.Name,
                    Price = post.Price,
                    Time = post.Time,
                    UtcTime = post.TimeUtc.ToString("s"),
                    Street = post.Street,
                    FacebookLink = post.FacebookLink,
                    IsOwner = post.UserId == userId,
                    Latitude = post.Latitude,
                    Longitude = post.Longitude,
                    PhotoId = post.PhotoId
                };

                if (post.PhotoId.HasValue)
                {
                    var photo = _uow.PhotoRepository.GetPhotoWithMinimalData(post.PhotoId.Value);
                    if (photo != null)
                        vm.ImageUrl = _photoService.GeneratePhotoUrl(photo.UploaderId, photo.Id);
                }

                return vm;
            }
        }
    }
}
