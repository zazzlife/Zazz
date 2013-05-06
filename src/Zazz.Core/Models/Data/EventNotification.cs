using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class EventNotification
    {
        [Key]
        public long NotificationId { get; set; }

        public int EventId { get; set; }

        [ForeignKey("NotificationId")]
        public Notification Notification { get; set; }

        [ForeignKey("EventId")]
        public ZazzEvent Event { get; set; }
    }
}