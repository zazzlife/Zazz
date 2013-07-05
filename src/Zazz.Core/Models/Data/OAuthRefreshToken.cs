using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class OAuthRefreshToken : BaseEntity
    {
        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("OAuthClient")]
        public int OAuthClientId { get; set; }

        [MaxLength(4000)]
        public string VerificationCode { get; set; }

        public ICollection<OAuthRefreshTokenScope> Scopes { get; set; }

        public virtual User User { get; set; }

        public virtual OAuthClient OAuthClient { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}