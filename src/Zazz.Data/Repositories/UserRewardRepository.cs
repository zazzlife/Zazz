using System;
using System.Data.Entity;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class UserRewardRepository : BaseRepository<UserReward>, IUserRewardRepository
    {
        public UserRewardRepository(DbContext dbContext)
            : base(dbContext)
        { }

        protected override int GetItemId(UserReward item)
        {
            throw new InvalidOperationException("You must provide the Id for updating. Use insert graph for insert");
        }

        public IQueryable<UserReward> GetRewards(int userId, int clubId)
        {
            return DbSet.Where(r => r.UserId == userId)
                        .Where(r => r.Reward.ClubId == clubId);
        }

        public bool Exists(int userId, int rewardId)
        {
// ReSharper disable ReplaceWithSingleCallToAny
            return DbSet.Where(r => r.UserId == userId)
                        .Where(r => r.RewardId == rewardId)
                        .Any();
// ReSharper restore ReplaceWithSingleCallToAny
        }
    }
}