using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class UserPreferences : BaseEntity
    {
        [ForeignKey("Id")]
        public User User { get; set; }

        public bool SyncFbEvents { get; set; }

        public bool SyncFbPosts { get; set; }

        public bool SyncFbImages { get; set; }

        public bool SendSyncErrorNotifications { get; set; }

        public DateTime? LastSyncError { get; set; }

        public DateTime? LasySyncErrorEmailSent { get; set; } 
    }
}