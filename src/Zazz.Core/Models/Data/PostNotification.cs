using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class PostNotification
    {
        [Key]
        public long NotificationId { get; set; }

        public int PostId { get; set; }

        [ForeignKey("NotificationId")]
        public virtual Notification Notification { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }
    }
}