using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class ClubReward : BaseEntity
    {
        [ForeignKey("Club")]
        public int ClubId { get; set; }

        [MaxLength(100), Required]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Range(0, 1000000)]
        public int Cost { get; set; }

        public User Club { get; set; }

        public bool IsEnabled { get; set; }

        public virtual ICollection<UserReward> UserRewards { get; set; }
    }
}