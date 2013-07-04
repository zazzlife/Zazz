﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class OAuthRefreshToken : BaseEntity
    {
        [ForeignKey("User")]
        public int UserId { get; set; }

        [MaxLength(4000)]
        public string Token { get; set; }

        public ICollection<OAuthScope> Scopes { get; set; }

        public virtual User User { get; set; }
    }
}