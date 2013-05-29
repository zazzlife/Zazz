using System;
using Zazz.Core.Models;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models.Api
{
    public class ApiNotification
    {
        public long Id { get; set; }

        public int FriendId { get; set; }

        public string FriendDisplayName { get; set; }

        public PhotoLinks FriendDisplayPhoto { get; set; }

        public NotificationType NotificationType { get; set; }

        public bool IsRead { get; set; }

        public DateTime Time { get; set; }

        public ApiPhoto Photo { get; set; }

        public ApiPost Post { get; set; }

        public ApiEvent Event { get; set; }
    }
}