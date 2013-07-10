using System.ComponentModel.DataAnnotations;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models
{
    public class ClubRewardScenarioViewModel
    {
        public int ScenarioId { get; set; }

        public PointRewardScenario Scenario { get; set; }

        [Range(1, 1000)]
        public int Amount { get; set; }
    }
}