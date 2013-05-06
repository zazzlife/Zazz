using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class CommentNotification
    {
        [Key]
        public long NotificationId { get; set; }

        public int CommentId { get; set; }

        [ForeignKey("NotificationId")]
        public Notification Notification { get; set; }

        [ForeignKey("CommentId")]
        public Comment Comment { get; set; }
    }
}