using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class ClubReward : BaseEntity
    {
        [ForeignKey("Club")]
        public int ClubId { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public int Cost { get; set; }

        public User Club { get; set; }
    }
}