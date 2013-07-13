using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Models.Data
{
    public class ClubPointRewardScenario : BaseEntity
    {
        [ForeignKey("Club")]
        public int ClubId { get; set; }

        public PointRewardScenario Scenario { get; set; }
        
        public int Amount { get; set; }

        public int MondayAmount { get; set; }

        public int TuesdayAmount { get; set; }

        public int WednesdayAmount { get; set; }

        public int ThursdayAmount { get; set; }

        public int FridayAmount { get; set; }

        public int SaturdayAmount { get; set; }

        public int SundayAmount { get; set; }

        public User Club { get; set; }
    }
}