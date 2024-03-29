﻿using System;
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
        /// Use this property to mention another user in the notification.
        /// </summary>
        [ForeignKey("UserBId")]
        public User UserB { get; set; }

        public PostNotification PostNotification { get; set; }

        public EventNotification EventNotification { get; set; }

        public CommentNotification CommentNotification { get; set; }

        //public PhotoNotification PhotoNotification { get; set; }

        /// <summary>
        /// User that receives the notification.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Use this property to mention another user in the notification.
        /// </summary>
        public int UserBId { get; set; }

        public NotificationType NotificationType { get; set; }

        public DateTime Time { get; set; }

        public bool IsRead { get; set; }
    }
}