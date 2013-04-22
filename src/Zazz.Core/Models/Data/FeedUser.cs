using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    [Table("Feed_User")]
    public class FeedUser : BaseEntity
    {
        [ForeignKey("FeedId")]
        public virtual Feed Feed { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public int FeedId { get; set; }

        public int UserId { get; set; }
    }
}