using System;
using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models.Api
{
    public class ApiNotification
    {
        public long NotificationId { get; set; }

        public int UserId { get; set; }

        public string DisplayName { get; set; }

        public PhotoLinks DisplayPhoto { get; set; }

        public NotificationType NotificationType { get; set; }

        public bool IsRead { get; set; }

        public DateTime Time { get; set; }

        public ApiPhoto Photo { get; set; }

        public ApiPost Post { get; set; }

        public ApiEvent Event { get; set; }
    }
}