using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Models;

namespace Zazz.Web.Controllers
{
    [Authorize]
    public class ClubRewardsController : BaseController
    {
        private readonly IUoW _uow;
        private readonly IClubRewardService _rewardService;

        public ClubRewardsController(IUserService userService, IPhotoService photoService,
                                     IDefaultImageHelper defaultImageHelper, IUoW uow,
            IClubRewardService rewardService)
            : base(userService, photoService, defaultImageHelper)
        {
            _uow = uow;
            _rewardService = rewardService;
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

        [HttpGet]
        public ActionResult Scenarios()
        {
            var currentUser = UserService.GetUser(User.Identity.Name);
            if (currentUser.AccountType == AccountType.User)
                return RedirectToAction("List");

            var scenarios = _uow.ClubPointRewardScenarioRepository.GetAll()
                                .Where(s => s.ClubId == currentUser.Id)
                                .Select(s => new ClubRewardScenarioViewModel
                                             {
                                                 Amount = s.Amount,
                                                 ScenarioId = s.Id,
                                                 Scenario = s.Scenario
                                             })
                                .ToList();

            return View(scenarios);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Scenarios(int amount, PointRewardScenario scenario)
        {
            try
            {
                var currentUser = UserService.GetUser(User.Identity.Name);
                if (currentUser.AccountType == AccountType.User)
                    return RedirectToAction("List");

                var rewardScenario = new ClubPointRewardScenario
                                     {
                                         Amount = amount,
                                         ClubId = currentUser.Id,
                                         Scenario = scenario
                                     };

                _rewardService.AddRewardScenario(rewardScenario);

                var scenarios = _uow.ClubPointRewardScenarioRepository.GetAll()
                                    .Where(s => s.ClubId == currentUser.Id)
                                    .Select(s => new ClubRewardScenarioViewModel
                                                 {
                                                     Amount = s.Amount,
                                                     ScenarioId = s.Id,
                                                     Scenario = s.Scenario
                                                 })
                                    .ToList();

                return View(scenarios);
            }
            catch (AlreadyExistsException)
            {
                ShowAlert("A scenario with the same condition already exists.", AlertType.Error);
                return Scenarios();
            }
        }

        [HttpGet]
        public ActionResult EditScenario(int id)
        {
            if (id == 0)
                throw new HttpException(404, "not found");

            var currentUserId = GetCurrentUserId();
            var scenario = _uow.ClubPointRewardScenarioRepository.GetById(id);
            if (scenario == null || currentUserId != scenario.ClubId)
                throw new HttpException(404, "not found");

            var vm = new ClubRewardScenarioViewModel
                     {
                         Amount = scenario.Amount,
                         ScenarioId = id,
                         Scenario = scenario.Scenario
                     };

            return View("EditScenario", vm);
        }

        [HttpPost]
        public ActionResult EditScenario(int id, ClubRewardScenarioViewModel vm)
        {
            if (id == 0)
                throw new HttpException(404, "not found");

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUserId = GetCurrentUserId();
                    _rewardService.ChangeRewardAmount(id, currentUserId, vm.Amount);

                    return RedirectToAction("Scenarios");
                }
                catch (NotFoundException)
                {
                    throw new HttpException(404, "not found");
                }
                catch (SecurityException)
                {
                    throw new HttpException(404, "not found");
                }
            }
            else
            {
                return View("EditScenario", vm);
            }
        }

        public ActionResult RemoveScenario()
        {
            return View();
        }
    }
}
