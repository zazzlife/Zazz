using System;
using System.ComponentModel.DataAnnotations.Schema;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Models.Data
{
    public class Notification : BaseEntityLong
    {
        /// <summary>
        /// User that receives the notification.
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        /// <summary>
        /// Use this property if you need to mention another user in the notification.
        /// </summary>
        [ForeignKey("UserBId")]
        public User UserB { get; set; }

        [ForeignKey("PhotoId")]
        public Photo Photo { get; set; }

        [ForeignKey("PostId")]
        public Post Post { get; set; }

        [ForeignKey("EventId")]
        public ZazzEvent Event { get; set; }

        /// <summary>
        /// User that receives the notification.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Use this property if you need to mention another user in the notification.
        /// </summary>
        public int? UserBId { get; set; }

        public int? PhotoId { get; set; }

        public int? PostId { get; set; }

        public int? EventId { get; set; }

        public NotificationType NotificationType { get; set; }

        public DateTime Time { get; set; }

        public bool IsRead { get; set; }
    }
}