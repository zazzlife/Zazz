using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models
{
    public class ScenariosViewModel
    {
        public int ScenarioId { get; set; }

        public PointRewardScenario Scenario { get; set; }

        public int Amount { get; set; }
    }
}