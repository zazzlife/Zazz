using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    [Table("Stat_Users")]
    public class StatUser
    {
        [ForeignKey("CategoryStatId")]
        public virtual CategoryStat CategoryStat { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Key, Column(Order = 0)]
        public int CategoryStatId { get; set; }

        [Key, Column(Order = 1)]
        public int UserId { get; set; }
    }
}