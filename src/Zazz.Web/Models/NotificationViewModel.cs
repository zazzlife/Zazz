using System;
using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models
{
    public class NotificationViewModel
    {
        public long NotificationId { get; set; }

        public int UserId { get; set; }

        public string UserDisplayName { get; set; }

        public PhotoLinks UserPhoto { get; set; }

        public int ItemId { get; set; }

        public NotificationType NotificationType { get; set; }

        public bool IsRead { get; set; }

        public string EventName { get; set; }

        public PhotoViewModel PhotoViewModel { get; set; }

        public DateTime Time { get; set; }
    }
}