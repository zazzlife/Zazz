using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Models.Data
{
    public class User : BaseEntity
    {
        public User()
        {
            LinkedAccounts = new HashSet<LinkedAccount>();
            Weeklies = new HashSet<Weekly>();
        }

        [MaxLength(50), Required]
        public string Username { get; set; }

        [MaxLength(200), Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [MaxLength(50)]
        public string Password { get; set; }

        public AccountType AccountType { get; set; }

        public DateTime LastActivity { get; set; }

        [Column(TypeName = "Date")]
        public DateTime JoinedDate { get; set; }

        public bool IsConfirmed { get; set; }

        public virtual UserValidationToken UserValidationToken { get; set; }

        public virtual ICollection<LinkedAccount> LinkedAccounts { get; set; }

        public virtual UserPreferences Preferences { get; set; }

        public virtual UserDetail UserDetail { get; set; }

        public virtual ClubDetail ClubDetail { get; set; }

        public virtual UserReceivedLikes ReceivedLikesCount { get; set; }

        public int? ProfilePhotoId { get; set; }

        public virtual ICollection<Weekly> Weeklies { get; set; }

        public virtual ICollection<Follow> Follows { get; set; }

        public virtual ICollection<Follow> Followers { get; set; }
    }
}