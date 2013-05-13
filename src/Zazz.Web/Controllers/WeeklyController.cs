using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    [Authorize]
    public class WeeklyController : Controller
    {
        private readonly IUserService _userService;

        public WeeklyController(IUserService userService)
        {
            _userService = userService;
        }

        public void New(WeeklyViewModel vm)
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
                                  PhotoId = vm.PhotoId
                              });
        }

        public void Edit(WeeklyViewModel vm)
        {

        }
    }
}
