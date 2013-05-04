using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    [Table("Tag_Users")]
    public class TagUser
    {
        [Key, Column(Order = 0)]
        public int TagStatId { get; set; }

        [Key, Column(Order = 1)]
        public int UserId { get; set; }

        public virtual TagStat TagStat { get; set; }

        public virtual User User { get; set; }
    }
}