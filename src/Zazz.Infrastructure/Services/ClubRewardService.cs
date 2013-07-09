using System;
using System.Security;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Infrastructure.Services
{
    public class ClubRewardService : IClubRewardService
    {
        private readonly IUoW _uow;

        public ClubRewardService(IUoW uow)
        {
            _uow = uow;
        }

        public void AddRewardScenario(ClubPointRewardScenario scenario)
        {
            var scenarioExists = _uow.ClubPointRewardScenarioRepository.Exists(scenario.ClubId, scenario.Scenario);
            if (scenarioExists)
                throw new AlreadyExistsException();

            _uow.ClubPointRewardScenarioRepository.InsertGraph(scenario);
            _uow.SaveChanges();
        }

        public void ChangeRewardAmount(int scenarioId, int currentUserId, int amount)
        {
            var scenario = _uow.ClubPointRewardScenarioRepository.GetById(scenarioId);
            if (scenario == null)
                throw new NotFoundException();

            if (scenario.ClubId != currentUserId)
                throw new SecurityException();

            scenario.Amount = amount;
            _uow.SaveChanges();
        }

        public void RemoveRewardScenario(int scenarioId, int currentUserId)
        {
            var scenario = _uow.ClubPointRewardScenarioRepository.GetById(scenarioId);
            if (scenario == null)
                return;

            if (scenario.ClubId != currentUserId)
                throw new SecurityException();

            _uow.ClubPointRewardScenarioRepository.Remove(scenario);
            _uow.SaveChanges();
        }

        public void AddClubReward(ClubReward reward)
        {
            if (reward.ClubId == 0 || String.IsNullOrWhiteSpace(reward.Name))
                throw new ArgumentException();

            _uow.ClubRewardRepository.InsertGraph(reward);
            _uow.SaveChanges();
        }

        public void UpdateClubReward(int rewardId, int currentUserId, ClubReward newReward)
        {
            var reward = _uow.ClubRewardRepository.GetById(rewardId);
            if (reward == null)
                throw new NotFoundException();

            if (reward.ClubId != currentUserId)
                throw new SecurityException();

            reward.Cost = newReward.Cost;
            reward.Description = newReward.Description;
            reward.Name = newReward.Name;

            _uow.SaveChanges();
        }

        /// <summary>
        /// Disabling the reward, not removing it because of UserPointsHistory relationship
        /// </summary>
        /// <param name="rewardId"></param>
        /// <param name="currentUserId"></param>
        public void RemoveClubReward(int rewardId, int currentUserId)
        {
            var reward = _uow.ClubRewardRepository.GetById(rewardId);
            if (reward == null)
                return;

            if (reward.ClubId != currentUserId)
                throw new SecurityException();

            reward.IsEnabled = false;
            _uow.SaveChanges();
        }

        public void RewardUserPoints(int userId, int clubId, int amount, PointRewardScenario scenario)
        {
            _uow.UserPointRepository.ChangeUserPoints(userId, clubId, amount);
            
            var historyRecord = new UserPointHistory
                                {
                                    ChangedAmount = amount,
                                    ClubId = clubId,
                                    Date = DateTime.UtcNow,
                                    UserId = userId,
                                    RewardScenario = scenario
                                };

            _uow.UserPointHistoryRepository.InsertGraph(historyRecord);
            _uow.SaveChanges();
        }

        public UserReward RedeemPoints(int userId, ClubReward reward)
        {
            throw new System.NotImplementedException();
        }
    }
}