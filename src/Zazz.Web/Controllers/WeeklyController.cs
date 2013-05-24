using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure;
using Zazz.Infrastructure.Helpers;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    [Authorize]
    public class WeeklyController : Controller
    {
        private readonly IWeeklyService _weeklyService;
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;
        private readonly IDefaultImageHelper _defaultImageHelper;

        public WeeklyController(IWeeklyService weeklyService, IUserService userService,
            IPhotoService photoService, IDefaultImageHelper defaultImageHelper)
        {
            _weeklyService = weeklyService;
            _userService = userService;
            _photoService = photoService;
            _defaultImageHelper = defaultImageHelper;
        }

        public ActionResult New(WeeklyViewModel vm)
        {
            var userId = _userService.GetUserId(User.Identity.Name);

            _weeklyService.CreateWeekly(new Weekly
                                        {
                                            DayOfTheWeek = vm.DayOfTheWeek,
                                            Description = vm.Description,
                                            Name = vm.Name,
                                            PhotoId = vm.PhotoId,
                                            UserId = userId
                                        });

            vm.OwnerUserId = userId;
            vm.CurrentUserId = userId;

            vm.PhotoLinks = vm.PhotoId.HasValue
                                ? _photoService.GeneratePhotoUrl(userId, vm.PhotoId.Value)
                                : _defaultImageHelper.GetDefaultWeeklyImage();

            return View("_WeeklyItem", vm);
        }

        public ActionResult Edit(WeeklyViewModel vm)
        {
            var userId = _userService.GetUserId(User.Identity.Name);
            _weeklyService.EditWeekly(new Weekly
                                      {
                                          Id = vm.Id,
                                          DayOfTheWeek = vm.DayOfTheWeek,
                                          Description = vm.Description,
                                          Name = vm.Name,
                                          PhotoId = vm.PhotoId,
                                      }, userId);

            vm.OwnerUserId = userId;
            vm.CurrentUserId = userId;

            vm.PhotoLinks = vm.PhotoId.HasValue
                                ? _photoService.GeneratePhotoUrl(userId, vm.PhotoId.Value)
                                : _defaultImageHelper.GetDefaultWeeklyImage();

            return View("_WeeklyItem", vm);
        }

        public void Remove(int id)
        {
            var userId = _userService.GetUserId(User.Identity.Name);
            _weeklyService.RemoveWeekly(id, userId);
        }
    }
}
