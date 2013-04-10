using System;
using System.ComponentModel.DataAnnotations;

namespace Zazz.Core.Models.Data
{
    public class FacebookSyncRetry : BaseEntity
    {
        public long FacebookUserId { get; set; }

        [MaxLength(4000)]
        public string Path { get; set; }

        [MaxLength(4000)]
        public string Fields { get; set; }

        public DateTime LastTry { get; set; }
    }
}