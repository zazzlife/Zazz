﻿using System;
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
            var userId = GetCurrentUserId();
            var accountType = UserService.GetAccountType(userId);

            return View(accountType);
        }

        public ActionResult List(int? id)
        {
            if (!id.HasValue)
                id = GetCurrentUserId();

            var accountType = UserService.GetAccountType(id.Value);
            if (accountType == AccountType.User)
                throw new HttpException(404, "not found");

            var rewards = _uow.ClubRewardRepository
                              .GetAll()
                              .Where(c => c.ClubId == id.Value)
                              .ToList();

            return View(rewards);
        }

        public ActionResult Create()
        {
            var userId = GetCurrentUserId();

            var accountType = UserService.GetAccountType(userId);
            if (accountType == AccountType.User)
                throw new HttpException(404, "not found");

            return View("EditReward");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(ClubReward reward)
        {
            var userId = GetCurrentUserId();
            var accountType = UserService.GetAccountType(userId);
            if (accountType == AccountType.User)
                throw new HttpException(404, "not found");

            if (ModelState.IsValid)
            {
                reward.ClubId = userId;
                _rewardService.AddClubReward(reward);

                return RedirectToAction("List", new { id = userId });
            }

            return View("EditReward", reward);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var currentUserId = GetCurrentUserId();

            var reward = _uow.ClubRewardRepository.GetById(id);
            if (reward == null || reward.ClubId != currentUserId)
                throw new HttpException(404, "not found");

            return View("EditReward", reward);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(int id, ClubReward reward)
        {
            return View("EditReward");
        }

        public ActionResult Delete()
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

        public ActionResult RemoveScenario(int id)
        {
            if (id == 0)
                throw new HttpException(404, "not found");

            try
            {
                var currentUserId = GetCurrentUserId();
                _rewardService.RemoveRewardScenario(id, currentUserId);

                return RedirectToAction("Scenarios");
            }
            catch (SecurityException)
            {
                throw new HttpException(404, "not found");
            }
        }
    }
}
