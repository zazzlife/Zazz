using System;
using System.Data.Entity;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Data.Repositories
{
    public class ClubPointRewardScenarioRepository : BaseRepository<ClubPointRewardScenario>,
                                                     IClubPointRewardScenarioRepository
    {
        public ClubPointRewardScenarioRepository(DbContext dbContext)
            : base(dbContext)
        { }

        public ClubPointRewardScenario Get(int clubId, PointRewardScenario scenario)
        {
// ReSharper disable ReplaceWithSingleCallToAny
            return DbSet.Where(c => c.ClubId == clubId)
                        .Where(c => c.Scenario == scenario)
                        .SingleOrDefault();
// ReSharper restore ReplaceWithSingleCallToAny
        }

        public bool Exists(int clubId, PointRewardScenario scenario)
        {
// ReSharper disable ReplaceWithSingleCallToAny
            return DbSet.Where(c => c.ClubId == clubId)
                        .Where(c => c.Scenario == scenario)
                        .Any();
// ReSharper restore ReplaceWithSingleCallToAny
        }
    }
}