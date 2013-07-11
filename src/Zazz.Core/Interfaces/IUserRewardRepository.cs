using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IUserRewardRepository : IRepository<UserReward>
    {
        bool Exists(int userId, int rewardId);
    }
}