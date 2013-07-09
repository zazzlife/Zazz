using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Interfaces
{
    public interface IClubRewardService
    {
        void AddRewardScenario(ClubPointRewardScenario scenario);

        void ChangeRewardAmount(int scenarioId, int currentUserId, int amount);

        void RemoveRewardScenario(int scenarioId, int currentUserId);


        void AddClubReward(ClubReward reward);

        void UpdateClubReward(int rewardId, int currentUserId, ClubReward newReward);

        void RemoveClubReward(int rewardId, int currentUserId);


        UserPoint RewardUserPoints(int clubId, int userId, int amount, PointRewardScenario scenario);

        UserReward RedeemPoints(int userId, ClubReward reward);
    }
}