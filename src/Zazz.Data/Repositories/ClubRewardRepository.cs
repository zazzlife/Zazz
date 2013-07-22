using System;
using System.Data.Entity;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class ClubRewardRepository : BaseRepository<ClubReward>, IClubRewardRepository
    {
        public ClubRewardRepository(DbContext dbContext)
            : base(dbContext)
        { }
    }
}