using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface IUserRewardRepository : IRepository<UserReward>
    {
        IQueryable<UserReward> GetRewards(int userId, int clubId);

        bool Exists(int userId, int rewardId);
    }
}