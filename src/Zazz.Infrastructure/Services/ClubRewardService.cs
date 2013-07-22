using System;
using System.Linq;
using System.Security;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
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

        public void ChangeRewardAmount(ClubPointRewardScenario updatedScenario, int currentUserId)
        {
            var scenario = _uow.ClubPointRewardScenarioRepository.GetById(updatedScenario.Id);
            if (scenario == null)
                throw new NotFoundException();

            if (scenario.ClubId != currentUserId)
                throw new SecurityException();

            scenario.MondayAmount = updatedScenario.MondayAmount;
            scenario.TuesdayAmount = updatedScenario.TuesdayAmount;
            scenario.WednesdayAmount = updatedScenario.WednesdayAmount;
            scenario.ThursdayAmount = updatedScenario.ThursdayAmount;
            scenario.FridayAmount = updatedScenario.FridayAmount;
            scenario.SaturdayAmount = updatedScenario.SaturdayAmount;
            scenario.SundayAmount = updatedScenario.SundayAmount;

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
            if (reward.ClubId == 0 || String.IsNullOrWhiteSpace(reward.Name) || reward.Cost < 0)
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
        
        public void DisableClubReward(int rewardId, int currentUserId)
        {
            var reward = _uow.ClubRewardRepository.GetById(rewardId);
            if (reward == null)
                return;

            if (reward.ClubId != currentUserId)
                throw new SecurityException();

            reward.IsEnabled = false;
            _uow.SaveChanges();
        }

        public void EnableClubReward(int rewardId, int currentUserId)
        {
            var reward = _uow.ClubRewardRepository.GetById(rewardId);
            if (reward == null)
                return;

            if (reward.ClubId != currentUserId)
                throw new SecurityException();

            reward.IsEnabled = true;
            _uow.SaveChanges();
        }

        public void AwardUserPoints(int userId, int clubId, int amount, PointRewardScenario scenario)
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

        public UserReward RedeemPoints(int userId, int rewardId)
        {
            var reward = _uow.ClubRewardRepository.GetById(rewardId);
            if (reward == null)
                throw new NotFoundException();

            var amountToRemove = reward.Cost*-1;
            var userPoints = _uow.UserPointRepository.GetAll(userId, reward.ClubId)
                                 .SingleOrDefault();

            if (userPoints == null || userPoints.Points < reward.Cost)
                throw new NotEnoughPointsException();

            var recordExists = _uow.UserRewardRepository.Exists(userId, rewardId);
            if (recordExists)
                throw new AlreadyExistsException();

            var pointsHistoryRecord = new UserPointHistory
                                {
                                    ChangedAmount = amountToRemove,
                                    ClubId = reward.ClubId,
                                    UserId = userId,
                                    Date = DateTime.UtcNow,
                                    RewardId = reward.Id
                                };

            var rewardsHistoryRecord = new UserRewardHistory
                                       {
                                           Date = DateTime.UtcNow,
                                           EditorUserId = userId,
                                           RewardId = rewardId,
                                           UserId = userId
                                       };

            var userReward = new UserReward
                             {
                                 RedeemedDate = DateTime.UtcNow,
                                 RewardId = reward.Id,
                                 UserId = userId
                             };

            _uow.UserPointHistoryRepository.InsertGraph(pointsHistoryRecord);
            _uow.UserRewardRepository.InsertGraph(userReward);
            _uow.UserRewardHistoryRepository.InsertGraph(rewardsHistoryRecord);
            
            userPoints.Points += amountToRemove;

            _uow.SaveChanges();

            return userReward;
        }

        public void RemoveUserReward(int rewardId, int currentUserId)
        {
            var reward = _uow.UserRewardRepository.GetById(rewardId);
            if (reward == null)
                throw new NotFoundException();

            if (currentUserId != reward.UserId && currentUserId != reward.Reward.ClubId)
                throw new SecurityException();

            var historyRecord = new UserRewardHistory
                                {
                                    Date = DateTime.UtcNow,
                                    EditorUserId = currentUserId,
                                    RewardId = rewardId,
                                    UserId = reward.UserId
                                };

            _uow.UserRewardRepository.Remove(reward);
            _uow.UserRewardHistoryRepository.InsertGraph(historyRecord);
            _uow.SaveChanges();
        }
    }
}