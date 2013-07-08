using System;
using System.Data.Entity;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class ClubRewardRepository : BaseRepository<ClubReward>, IClubRewardRepository
    {
        public ClubRewardRepository(DbContext dbContext)
            : base(dbContext)
        { }

        protected override int GetItemId(ClubReward item)
        {
            throw new InvalidOperationException("You must provide the Id for updating. Use insert graph for insert");
        }
    }
}