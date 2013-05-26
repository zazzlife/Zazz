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
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure;
using Zazz.Infrastructure.Helpers;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    public class EventController : BaseController
    {
        private readonly IEventService _eventService;
        private readonly IUoW _uow;
        private readonly IDefaultImageHelper _defaultImageHelper;

        private const int PAGE_SIZE = 10;

        public EventController(IUserService userService, IEventService eventService,
            IUoW uow, IPhotoService photoService, IDefaultImageHelper defaultImageHelper)
            : base(userService, photoService)
        {
            _eventService = eventService;
            _uow = uow;
            _defaultImageHelper = defaultImageHelper;
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            var userDisplayName = String.Empty;
            var userPhoto = _defaultImageHelper.GetUserDefaultImage(Gender.NotSpecified);
            if (Request.IsAuthenticated)
            {
                var userId = _userService.GetUserId(User.Identity.Name);
                userDisplayName = _userService.GetUserDisplayName(userId);
                userPhoto = _photoService.GetUserImageUrl(userId);
            }

            var vm = new EventListViewModel
                     {
                         CurrentUserDisplayName = userDisplayName,
                         CurrentUserPhoto = userPhoto,
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


            return GetEvents(firstDayOfMonth, firstDayOfWeek, lastDayOfWeek.AddDays(1), lastDayOfMonth, page);
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
                                              FacebookPhotoUrl = e.FacebookPhotoLink,
                                              IsDateOnly = e.IsDateOnly,
                                              FacebookEventId = e.FacebookEventId
                                          }).ToList();

            foreach (var e in events)
            {
                if (e.IsFacebookEvent)
                {
                    e.ImageUrl = new PhotoLinks(e.FacebookPhotoUrl);
                    continue;
                }

                if (!e.PhotoId.HasValue)
                {
                    e.ImageUrl = _defaultImageHelper.GetDefaultEventImage();
                    continue;
                }

                var photo = _uow.PhotoRepository.GetPhotoWithMinimalData(e.PhotoId.Value);
                if (photo == null)
                {
                    e.ImageUrl = _defaultImageHelper.GetDefaultEventImage();
                    continue;
                }

                e.ImageUrl = _photoService.GeneratePhotoUrl(photo.UserId, photo.Id);
            }

            var eventsCount = _uow.EventRepository.GetEventRange(from, to, from2, to2).Count();

            return new StaticPagedList<EventViewModel>(events, page, PAGE_SIZE, eventsCount);
        }

        [HttpGet, Authorize]
        public ActionResult Create()
        {
            ViewBag.FormAction = "Create";

            var userId = _userService.GetUserId(User.Identity.Name);
            var displayName = _userService.GetUserDisplayName(userId);
            var userPhoto = _photoService.GetUserImageUrl(userId);

            var vm = new EventDetailsPageViewModel
                     {
                         CurrentUserDisplayName = displayName,
                         CurrentUserPhoto = userPhoto,
                         EventViewModel = new EventViewModel
                                          {
                                              Time = DateTime.UtcNow,
                                              UtcTime = DateTime.UtcNow.ToString("s"),
                                              ImageUrl = _defaultImageHelper.GetDefaultAlbumImage()
                                          }
                     };

            return View("EditForm", vm);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize]
        public ActionResult Create(EventDetailsPageViewModel vm)
        {
            var userId = _userService.GetUserId(User.Identity.Name);
            if (userId == 0)
                throw new HttpException(401, "Unauthorized");

            if (ModelState.IsValid)
            {
                var zazzEvent = EventViewModelToZazzEvent(vm.EventViewModel, userId);
                zazzEvent.CreatedDate = DateTime.UtcNow;

                _eventService.CreateEvent(zazzEvent);
                return Redirect("~/event/show/" + zazzEvent.Id);
            }

            vm.CurrentUserDisplayName = _userService.GetUserDisplayName(userId);
            vm.CurrentUserPhoto = _photoService.GetUserImageUrl(userId);

            ViewBag.FormAction = "Create";
            return View("EditForm", vm);
        }

        [HttpGet, Authorize]
        public ActionResult Show(int id)
        {
            var eventVm = GetEvent(id, false);

            var userId = _userService.GetUserId(User.Identity.Name);
            var displayName = _userService.GetUserDisplayName(userId);
            var userPhoto = _photoService.GetUserImageUrl(userId);

            var vm = new EventDetailsPageViewModel
                     {
                         EventViewModel = eventVm,
                         CurrentUserDisplayName = displayName,
                         CurrentUserPhoto = userPhoto
                     };

            return View(vm);
        }

        [HttpGet, Authorize]
        public ActionResult Edit(int id)
        {
            var userId = _userService.GetUserId(User.Identity.Name);
            var displayName = _userService.GetUserDisplayName(userId);
            var userPhoto = _photoService.GetUserImageUrl(userId);

            var vm = new EventDetailsPageViewModel
                     {
                         EventViewModel = GetEvent(id, true),
                         CurrentUserDisplayName = displayName,
                         CurrentUserPhoto = userPhoto
                     };

            ViewBag.FormAction = "Edit";

            return View("EditForm", vm);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult Edit(int id, EventDetailsPageViewModel vm)
        {
            var userId = _userService.GetUserId(User.Identity.Name);

            if (ModelState.IsValid)
            {
                var post = EventViewModelToZazzEvent(vm.EventViewModel, userId);
                post.Id = id;
                post.CreatedDate = vm.EventViewModel.CreatedDate.Value;

                _eventService.UpdateEvent(post, userId);

                return Redirect("~/event/show/" + id);
            }

            var displayName = _userService.GetUserDisplayName(userId);
            var userPhoto = _photoService.GetUserImageUrl(userId);

            vm.CurrentUserDisplayName = displayName;
            vm.CurrentUserPhoto = userPhoto;

            return View("EditForm", vm);
        }

        [Authorize]
        public ActionResult Remove(int id)
        {
            var userId = _userService.GetUserId(User.Identity.Name);
            _eventService.DeleteEvent(id, userId);

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

        private EventViewModel GetEvent(int id, bool ownerOnly)
        {
            var userId = _userService.GetUserId(User.Identity.Name);

            var zazzEvent = _eventService.GetEvent(id);
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
                         ImageUrl = zazzEvent.IsFacebookEvent
                                        ? new PhotoLinks(zazzEvent.FacebookPhotoLink)
                                        : _defaultImageHelper.GetDefaultAlbumImage(),
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
