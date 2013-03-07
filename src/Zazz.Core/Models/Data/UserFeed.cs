using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class UserFeed : BaseEntity
    {
        [ForeignKey("UserId")]
        public User User { get; set; }

        public int? UserId { get; set; }

        [ForeignKey("PostId")]
        public Post Post { get; set; }

        public int? PostId { get; set; }

        public FeedType FeedType { get; set; }
    }
}