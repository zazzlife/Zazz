using System;
using System.Data.Entity;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class UserRewardHistoryRepository : BaseRepository<UserRewardHistory>, IUserRewardHistoryRepository
    {
        public UserRewardHistoryRepository(DbContext dbContext) : base(dbContext)
        {}

        protected override int GetItemId(UserRewardHistory item)
        {
            throw new NotImplementedException();
        }
    }
}