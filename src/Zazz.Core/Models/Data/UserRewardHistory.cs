using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    [Table("UserRewardsHistory")]
    public class UserRewardHistory : BaseEntity
    {
        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Club")]
        public int ClubId { get; set; }

        [ForeignKey("Editor")]
        public int EditorUserId { get; set; }

        [ForeignKey("Reward")]
        public int RewardId { get; set; }

        public User User { get; set; }

        public User Club { get; set; }

        public User Editor { get; set; }

        public ClubReward Reward { get; set; }

        public DateTime Date { get; set; }
    }
}