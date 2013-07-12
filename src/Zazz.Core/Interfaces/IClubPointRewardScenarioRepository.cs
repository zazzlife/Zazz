using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Interfaces
{
    public interface IClubPointRewardScenarioRepository : IRepository<ClubPointRewardScenario>
    {
        ClubPointRewardScenario Get(int clubId, PointRewardScenario scenario);

        bool Exists(int clubId, PointRewardScenario scenario);
    }
}