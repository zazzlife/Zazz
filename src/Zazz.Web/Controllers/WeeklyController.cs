using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure;
using Zazz.Infrastructure.Helpers;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    [Authorize]
    public class WeeklyController : BaseController
    {
        private readonly IWeeklyService _weeklyService;

        public WeeklyController(IWeeklyService weeklyService, IUserService userService,
                                IPhotoService photoService, IDefaultImageHelper defaultImageHelper,
                                IStaticDataRepository staticDataRepository)
            : base(userService, photoService, defaultImageHelper, staticDataRepository)
        {
            _weeklyService = weeklyService;
        }

        public ActionResult New(WeeklyViewModel vm)
        {
            var userId = UserService.GetUserId(User.Identity.Name);

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
                                ? PhotoService.GeneratePhotoUrl(userId, vm.PhotoId.Value)
                                : DefaultImageHelper.GetDefaultWeeklyImage();

            return View("_WeeklyItem", vm);
        }

        public ActionResult Edit(WeeklyViewModel vm)
        {
            var userId = UserService.GetUserId(User.Identity.Name);
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
                                ? PhotoService.GeneratePhotoUrl(userId, vm.PhotoId.Value)
                                : DefaultImageHelper.GetDefaultWeeklyImage();

            return View("_WeeklyItem", vm);
        }

        public void Remove(int id)
        {
            var userId = UserService.GetUserId(User.Identity.Name);
            _weeklyService.RemoveWeekly(id, userId);
        }
    }
}
