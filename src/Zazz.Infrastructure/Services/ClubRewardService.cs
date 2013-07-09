using System;
using System.Security;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

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
            throw new System.NotImplementedException();
        }

        public void RemoveClubReward(int rewardId, int currentUserId)
        {
            throw new System.NotImplementedException();
        }

        public UserPoint RewardUserPoints(int clubId, int userId, int amount)
        {
            throw new System.NotImplementedException();
        }

        public UserReward RedeemPoints(int userId, ClubReward reward)
        {
            throw new System.NotImplementedException();
        }
    }
}