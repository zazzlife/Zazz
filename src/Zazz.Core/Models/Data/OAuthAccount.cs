using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class OAuthAccount : BaseEntity
    {
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public int UserId { get; set; }

        public long ProviderUserId { get; set; }

        [MaxLength(300)]
        public string AccessToken { get; set; }

        public OAuthProvider Provider { get; set; }

        public OAuthVersion OAuthVersion { get; set; }
    }
}