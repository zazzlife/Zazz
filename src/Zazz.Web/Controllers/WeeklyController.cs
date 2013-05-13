using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Infrastructure.Helpers;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    [Authorize]
    public class WeeklyController : Controller
    {
        private readonly IUoW _uow;
        private readonly IUserService _userService;
        private readonly IPhotoService _photoService;

        public WeeklyController(IUoW uow, IUserService userService, IPhotoService photoService)
        {
            _uow = uow;
            _userService = userService;
            _photoService = photoService;
        }

        public ActionResult New(WeeklyViewModel vm)
        {
            var user = _userService.GetUser(User.Identity.Name,
                includeDetails: false,
                includeClubDetails: false,
                includeWeeklies: true);

            if (user == null || user.AccountType != AccountType.ClubAdmin)
                throw new HttpException(401, "Not Allowed");

            user.Weeklies.Add(new Weekly
                              {
                                  Description = vm.Description,
                                  DayOfTheWeek = vm.DayOfTheWeek,
                                  Name = vm.Name,
                                  PhotoId = vm.PhotoId,
                              });

            vm.OwnerUserId = user.Id;
            vm.CurrentUserId = user.Id;

            vm.PhotoLinks = vm.PhotoId.HasValue
                                ? _photoService.GeneratePhotoUrl(user.Id, vm.PhotoId.Value)
                                : DefaultImageHelper.GetDefaultWeeklyImage();

            _uow.SaveChanges();
            return View("_WeeklyItem", vm);
        }

        public void Edit(WeeklyViewModel vm)
        {

        }
    }
}
