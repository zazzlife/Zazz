using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class FeedUserId : BaseEntity
    {
        [ForeignKey("FeedId")]
        public Feed Feed { get; set; }

        public int FeedId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        public int UserId { get; set; }
    }
}