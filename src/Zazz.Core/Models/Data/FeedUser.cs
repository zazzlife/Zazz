using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    [Table("Feeds_Users")]
    public class FeedUser
    {
        [ForeignKey("FeedId")]
        public virtual Feed Feed { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Key, Column(Order = 0)]
        public int FeedId { get; set; }

        [Key, Column(Order = 1)]
        public int UserId { get; set; }
    }
}