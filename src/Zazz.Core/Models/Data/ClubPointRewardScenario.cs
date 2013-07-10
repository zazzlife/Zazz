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

        [Range(1, 1000)]
        public int Amount { get; set; }

        public User Club { get; set; }
    }
}