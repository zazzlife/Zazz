using System;
using System.ComponentModel.DataAnnotations.Schema;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Models.Data
{
    public class Notification : BaseEntityLong
    {
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("AcceptedFollowUserId")]
        public User AcceptedFollowUser { get; set; }

        [ForeignKey("PhotoId")]
        public Photo Photo { get; set; }

        [ForeignKey("PostId")]
        public Post Post { get; set; }

        [ForeignKey("EventId")]
        public ZazzEvent Event { get; set; }

        public int UserId { get; set; }

        public int? AcceptedFollowUserId { get; set; }

        public int? PhotoId { get; set; }

        public int? PostId { get; set; }

        public int? EventId { get; set; }

        public NotificationType NotificationType { get; set; }

        public DateTime Time { get; set; }

        public bool IsRead { get; set; }
    }
}