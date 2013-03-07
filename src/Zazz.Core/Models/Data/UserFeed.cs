using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class UserFeed : BaseEntity
    {
        [ForeignKey("UserId")]
        public User User { get; set; }

        public int? UserId { get; set; }

        [ForeignKey("UserEventId")]
        public UserEvent UserEvent { get; set; }

        public int? UserEventId { get; set; }

        public FeedType FeedType { get; set; }
    }
}