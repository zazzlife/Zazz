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
                         MonthEvents = new EventListSideViewModel
                                       {
                                           Events = GetMonthEvents(),
                                           EventsRange = EventRange.Month
                                       },

                         WeekEvents = new EventListSideViewModel
                                      {
                                          Events = GetWeekEvents(),
                                          EventsRange = EventRange.Week
                                      }
                     };

            return View(vm);
        }

        public ActionResult Week(int page)
        {
            var vm = new EventListSideViewModel
                     {
                         Events = GetWeekEvents(page),
                         EventsRange = EventRange.Week
                     };

            return View("_EventsListPartial", vm);
        }

        public ActionResult Month(int page)
        {
            var vm = new EventListSideViewModel
            {
                Events = GetMonthEvents(page),
                EventsRange = EventRange.Month
            };

            return View("_EventsListPartial", vm);
        }

        private void GetThisWeek(out DateTime firstDayOfWeek, out DateTime lastDayOfWeek)
        {
            var today = DateTime.UtcNow;

            var delta = DayOfWeek.Sunday - today.DayOfWeek;
            firstDayOfWeek = today.AddDays(delta).Date;

            delta = DayOfWeek.Saturday - today.DayOfWeek;
            lastDayOfWeek = today.AddDays(delta).Date;
        }

        private IPagedList<EventViewModel> GetWeekEvents(int page = 1)
        {
            DateTime firstDayOfWeek;
            DateTime lastDayOfWeek;
            GetThisWeek(out firstDayOfWeek, out lastDayOfWeek);

            return GetEvents(firstDayOfWeek, lastDayOfWeek, null, null, page);
        }

        private IPagedList<EventViewModel> GetMonthEvents(int page = 1)
        {
            DateTime firstDayOfWeek;
            DateTime lastDayOfWeek;
            GetThisWeek(out firstDayOfWeek, out lastDayOfWeek);

            var today = DateTime.UtcNow;
            var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);

            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1).Date;
            var lastDayOfMonth = new DateTime(today.Year, today.Month, daysInMonth).Date;
            

            return GetEvents(firstDayOfMonth, firstDayOfWeek, lastDayOfWeek, lastDayOfMonth, page);
        }

        private IPagedList<EventViewModel> GetEvents(DateTime from, DateTime to, DateTime? from2, DateTime? to2, int page)
        {
            var skip = (page - 1) * PAGE_SIZE;

            var events = _uow.EventRepository.GetEventRange(from, to, from2, to2)
                             .OrderBy(e => e.TimeUtc)
                             .Skip(skip)
                             .Take(PAGE_SIZE)
                             .Select(e => new EventViewModel
                             {
                                 City = e.City,
                                 Id = e.Id,
                                 CreatedDate = e.CreatedDate,
                                 Description = e.Description,
                                 Latitude = e.Latitude,
                                 Longitude = e.Longitude,
                                 Location = e.Location,
                                 Name = e.Name,
                                 Price = e.Price,
                                 Street = e.Street,
                                 Time = e.Time,
                                 PhotoId = e.PhotoId,
                                 IsFacebookEvent = e.IsFacebookEvent,
                                 ImageUrl = e.IsFacebookEvent ? e.FacebookPhotoLink : null,
                                 IsDateOnly = e.IsDateOnly,
                                 FacebookEventId = e.FacebookEventId
                             }).ToList();

            foreach (var e in events)
            {

                if (!e.PhotoId.HasValue)
                    continue;

                var photo = _uow.PhotoRepository.GetPhotoWithMinimalData(e.PhotoId.Value);
                if (photo == null)
                    continue;

                e.ImageUrl = _photoService.GeneratePhotoUrl(photo.UserId, photo.Id);
            }

            var eventsCount = _uow.EventRepository.GetEventRange(from, to, from2, to2).Count();

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

                    var zazzEvent = EventViewModelToZazzEvent(vm, userId);
                    zazzEvent.CreatedDate = DateTime.UtcNow;

                    _eventService.CreateEvent(zazzEvent);
                    return Redirect("~/event/show/" + zazzEvent.Id);
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
                    var post = EventViewModelToZazzEvent(vm, userId);
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

        private static ZazzEvent EventViewModelToZazzEvent(EventViewModel vm, int userId)
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
                           TimeUtc = vm.Time.UtcDateTime,
                           Street = vm.Street,
                           Latitude = vm.Latitude,
                           Longitude = vm.Longitude
                       };


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

                var zazzEvent = await _eventService.GetEventAsync(id);
                if (zazzEvent == null)
                    throw new HttpException(404, "The requested entry was not found");

                if (ownerOnly && userId != zazzEvent.UserId)
                    throw new HttpException(401, "Unauthorized");

                var vm = new EventViewModel
                {
                    Id = zazzEvent.Id,
                    City = zazzEvent.City,
                    CreatedDate = zazzEvent.CreatedDate,
                    Description = zazzEvent.Description,
                    Location = zazzEvent.Location,
                    Name = zazzEvent.Name,
                    Price = zazzEvent.Price,
                    Time = zazzEvent.Time,
                    UtcTime = zazzEvent.TimeUtc.ToString("s"),
                    Street = zazzEvent.Street,
                    IsOwner = zazzEvent.UserId == userId,
                    Latitude = zazzEvent.Latitude,
                    Longitude = zazzEvent.Longitude,
                    PhotoId = zazzEvent.PhotoId,
                    IsFacebookEvent = zazzEvent.IsFacebookEvent,
                    ImageUrl = zazzEvent.IsFacebookEvent ? zazzEvent.FacebookPhotoLink : null,
                    IsDateOnly = zazzEvent.IsDateOnly,
                    FacebookEventId = zazzEvent.FacebookEventId
                };

                if (zazzEvent.PhotoId.HasValue)
                {
                    var photo = _uow.PhotoRepository.GetPhotoWithMinimalData(zazzEvent.PhotoId.Value);
                    if (photo != null)
                        vm.ImageUrl = _photoService.GeneratePhotoUrl(photo.UserId, photo.Id);
                }

                return vm;
            }
        }
    }
}
