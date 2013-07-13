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
            var userId = GetCurrentUserId();
            var accountType = UserService.GetAccountType(userId);

            return View(accountType);
        }

        public ActionResult List(int? id)
        {
            var currentUserId = GetCurrentUserId();
            if (!id.HasValue)
                id = currentUserId;

            var accountType = UserService.GetAccountType(id.Value);
            if (accountType == AccountType.User)
                throw new HttpException(404, "not found");

            var isCurrentUserOwner = id.Value == currentUserId;
            var currentUserAccountType = isCurrentUserOwner
                                             ? accountType
                                             : UserService.GetAccountType(currentUserId);
            var currentUserPoints = 0;
            if (!isCurrentUserOwner && currentUserAccountType == AccountType.User)
            {
                currentUserPoints = _uow.UserPointRepository.GetAll(currentUserId, id.Value)
                                        .Select(p => p.Points)
                                        .SingleOrDefault();
            }

            var vm = new ClubRewardsListViewModel
                     {
                         IsCurrentUserOwner = currentUserId == id.Value,
                         Rewards = _uow.ClubRewardRepository
                                       .GetAll()
                                       .Where(c => c.ClubId == id.Value)
                                       .Select(c => new ClubRewardViewModel
                                                    {
                                                        Id = c.Id,
                                                        Cost = c.Cost,
                                                        Description = c.Description,
                                                        IsEnabled = c.IsEnabled,
                                                        Name = c.Name,
                                                        AlreadyPurchased = isCurrentUserOwner 
                                                            ? false 
                                                            : currentUserAccountType == AccountType.Club
                                                                ? false
                                                                : c.UserRewards.Any(r => r.UserId == currentUserId)
                                                    })
                                       .ToList(),
                         CurrentUserAccountType = currentUserAccountType,
                         ClubName = UserService.GetUserDisplayName(id.Value),
                         CurrentUserPoints = currentUserPoints
                     };

            return View(vm);
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

            reward.IsEnabled = true;

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
            if (ModelState.IsValid)
            {
                reward.IsEnabled = true;

                try
                {
                    var currentUserId = GetCurrentUserId();
                    _rewardService.UpdateClubReward(id, currentUserId, reward);

                    return RedirectToAction("List");
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

            return View("EditReward", reward);
        }

        public ActionResult Disable(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                _rewardService.DisableClubReward(id, currentUserId);

                return RedirectToAction("List");
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

        public ActionResult Enable(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                _rewardService.EnableClubReward(id, currentUserId);

                return RedirectToAction("List");
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
                                                 ScenarioId = s.Id,
                                                 Scenario = s.Scenario,
                                                 MondayAmount = s.MondayAmount,
                                                 TuesdayAmount = s.TuesdayAmount,
                                                 WednesdayAmount = s.WednesdayAmount,
                                                 ThursdayAmount = s.ThursdayAmount,
                                                 FridayAmount = s.FridayAmount,
                                                 SaturdayAmount = s.SaturdayAmount,
                                                 SundayAmount = s.SundayAmount
                                             })
                                .ToList();

            return View(scenarios);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Scenarios(ClubRewardScenarioViewModel vm)
        {
            try
            {
                var currentUser = UserService.GetUser(User.Identity.Name);
                if (currentUser.AccountType == AccountType.User)
                    return RedirectToAction("List");

                var rewardScenario = new ClubPointRewardScenario
                                     {
                                         MondayAmount = vm.MondayAmount,
                                         TuesdayAmount = vm.TuesdayAmount,
                                         WednesdayAmount = vm.WednesdayAmount,
                                         ThursdayAmount = vm.ThursdayAmount,
                                         FridayAmount = vm.FridayAmount,
                                         SaturdayAmount = vm.SaturdayAmount,
                                         SundayAmount = vm.SundayAmount,
                                         ClubId = currentUser.Id,
                                         Scenario = vm.Scenario
                                     };

                _rewardService.AddRewardScenario(rewardScenario);

                var scenarios = _uow.ClubPointRewardScenarioRepository.GetAll()
                                    .Where(s => s.ClubId == currentUser.Id)
                                    .Select(s => new ClubRewardScenarioViewModel
                                                 {
                                                     ScenarioId = s.Id,
                                                     Scenario = s.Scenario,
                                                     MondayAmount = s.MondayAmount,
                                                     TuesdayAmount = s.TuesdayAmount,
                                                     WednesdayAmount = s.WednesdayAmount,
                                                     ThursdayAmount = s.ThursdayAmount,
                                                     FridayAmount = s.FridayAmount,
                                                     SaturdayAmount = s.SaturdayAmount,
                                                     SundayAmount = s.SundayAmount
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
                         ScenarioId = id,
                         Scenario = scenario.Scenario,
                         MondayAmount = scenario.MondayAmount,
                         TuesdayAmount = scenario.TuesdayAmount,
                         WednesdayAmount = scenario.WednesdayAmount,
                         ThursdayAmount = scenario.ThursdayAmount,
                         FridayAmount = scenario.FridayAmount,
                         SaturdayAmount = scenario.SaturdayAmount,
                         SundayAmount = scenario.SundayAmount
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

                    var updatedScenario = new ClubPointRewardScenario
                                          {
                                              Id = id,
                                              MondayAmount = vm.MondayAmount,
                                              TuesdayAmount = vm.TuesdayAmount,
                                              WednesdayAmount = vm.WednesdayAmount,
                                              ThursdayAmount = vm.ThursdayAmount,
                                              FridayAmount = vm.FridayAmount,
                                              SaturdayAmount = vm.SaturdayAmount,
                                              SundayAmount = vm.SundayAmount,
                                          };

                    var currentUserId = GetCurrentUserId();
                    _rewardService.ChangeRewardAmount(updatedScenario, currentUserId);

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

        [HttpGet, ActionName("Redeem")]
        public ActionResult ConfirmRedeem(int id)
        {
            if (id == 0)
                throw new HttpException(404, "not found");

            var reward = _uow.ClubRewardRepository.GetById(id);
            if (reward == null)
                throw new HttpException(404, "not found");

            return View(reward);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Redeem(int id, int clubId)
        {
            if (id == 0)
                throw new HttpException(404, "not found");

            try
            {
                var currentUserId = GetCurrentUserId();
                _rewardService.RedeemPoints(currentUserId, id);

                ShowAlert("Success!", AlertType.Success);
            }
            catch (NotFoundException)
            {
                throw new HttpException(404, "not found");
            }
            catch (NotEnoughPointsException)
            {
                ShowAlert("Not enough points.", AlertType.Error);
            }
            catch (AlreadyExistsException)
            {
                ShowAlert("You already have this reward", AlertType.Error);
            }

            return RedirectToAction("List", new {id = clubId});
        }

        public ActionResult ActiveRewards()
        {
            var currentUserId = GetCurrentUserId();
            var accountType = UserService.GetAccountType(currentUserId);
            if (accountType == AccountType.Club)
                throw new HttpException(404, "not found");

            var rewards = _uow.UserRewardRepository
                              .GetAll()
                              .Where(r => r.UserId == currentUserId)
                              .Select(r => new ClubRewardViewModel
                                           {
                                               ClubId = r.Reward.ClubId,
                                               Description = r.Reward.Description,
                                               Name = r.Reward.Name,
                                               RedeemedDate = r.RedeemedDate,
                                               ClubName = r.Reward.Club.ClubDetail.ClubName
                                           });

            return View(rewards);
        }
    }
}
