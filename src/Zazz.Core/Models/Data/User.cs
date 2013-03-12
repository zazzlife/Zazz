using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class User : BaseEntity
    {
        [StringLength(20, MinimumLength = 2, ErrorMessage = "{0} must be between {1} and {2} characters.")]
        public string Username { get; set; }

        [MaxLength(40), Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [MaxLength(28), Required, DataType("char")]
        public string Password { get; set; }

        public AccountType AccountType { get; set; }

        public DateTime LastActivity { get; set; }

        public bool IsConfirmed { get; set; }

        public virtual ValidationToken ValidationToken { get; set; }

        public virtual ICollection<OAuthAccount> LinkedAccounts { get; set; }

        public virtual ICollection<Album> Albums  { get; set; }

        public virtual ICollection<Photo> Photos { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<Follow> FollowingUsers { get; set; }

        public virtual ICollection<FollowRequest> SentFollowRequests { get; set; }

        public virtual UserDetail UserDetail { get; set; }

        public virtual ClubDetail ClubDetail { get; set; }
    }
}