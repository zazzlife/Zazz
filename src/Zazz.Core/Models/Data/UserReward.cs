using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class UserReward : BaseEntity
    {
        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Reward")]
        public int RewardId { get; set; }

        public int PointsSepnt { get; set; }

        public DateTime RedeemedDate { get; set; }

        public User User { get; set; }

        public ClubReward Reward { get; set; }
    }
}