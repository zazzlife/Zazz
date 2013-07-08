using System.ComponentModel.DataAnnotations.Schema;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Models.Data
{
    [Table("UserPointsHistory")]
    public class UserPointHistory : BaseEntityLong
    {
        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Club")]
        public int ClubId { get; set; }

        [ForeignKey("Reward")]
        public int RewardId { get; set; }

        public int ChangedAmount { get; set; }

        public PointRewardScenario RewardScenario { get; set; }

        public User User { get; set; }

        public User Club { get; set; }

        public ClubReward Reward { get; set; }
    }
}