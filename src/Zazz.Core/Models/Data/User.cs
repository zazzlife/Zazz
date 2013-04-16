using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class User : BaseEntity
    {
        public User()
        {
            LinkedAccounts = new List<OAuthAccount>();
        }

        [MaxLength(50), Required]
        public string Username { get; set; }

        [MaxLength(200), Required, DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [MaxLength(28), Required, DataType("char")]
        public string Password { get; set; }

        public AccountType AccountType { get; set; }

        public DateTime LastActivity { get; set; }

        public bool IsConfirmed { get; set; }

        public virtual ValidationToken ValidationToken { get; set; }

        public virtual ICollection<OAuthAccount> LinkedAccounts { get; set; }

        public virtual UserDetail UserDetail { get; set; }

        public virtual ClubDetail ClubDetail { get; set; }
    }
}