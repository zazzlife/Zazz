using System.ComponentModel.DataAnnotations;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models
{
    public class ClubRewardScenarioViewModel
    {
        public int ScenarioId { get; set; }

        public PointRewardScenario Scenario { get; set; }

        [Range(0, 1000)]
        public int MondayAmount { get; set; }

        [Range(0, 1000)]
        public int TuesdayAmount { get; set; }

        [Range(0, 1000)]
        public int WednesdayAmount { get; set; }

        [Range(0, 1000)]
        public int ThursdayAmount { get; set; }

        [Range(0, 1000)]
        public int FridayAmount { get; set; }

        [Range(0, 1000)]
        public int SaturdayAmount { get; set; }

        [Range(0, 1000)]
        public int SundayAmount { get; set; }
    }
}