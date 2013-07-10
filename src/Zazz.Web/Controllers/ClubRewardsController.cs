using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    [Authorize]
    public class ClubRewardsController : BaseController
    {
        private readonly IUoW _uow;

        public ClubRewardsController(IUserService userService, IPhotoService photoService,
                                     IDefaultImageHelper defaultImageHelper, IUoW uow)
            : base(userService, photoService, defaultImageHelper)
        {
            _uow = uow;
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            return View();
        }

        public ActionResult New()
        {
            return View();
        }

        public ActionResult Edit()
        {
            return View();
        }

        public ActionResult Remove()
        {
            return View();
        }

        public ActionResult Scenarios()
        {
            var currentUser = UserService.GetUser(User.Identity.Name);
            if (currentUser.AccountType == AccountType.User)
                return RedirectToAction("List");

            var scenarios = _uow.ClubPointRewardScenarioRepository.GetAll()
                                .Where(s => s.ClubId == currentUser.Id)
                                .Select(s => new ScenariosViewModel
                                             {
                                                 Amount = s.Amount,
                                                 ScenarioId = s.Id,
                                                 Scenario = s.Scenario
                                             })
                                .ToList();

            return View(scenarios);
        }

        public ActionResult AddScenario()
        {
            return View();
        }

        public ActionResult RemoveScenario()
        {
            return View();
        }
    }
}
