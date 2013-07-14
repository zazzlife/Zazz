using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class UserValidationToken : BaseEntity
    {
        [ForeignKey("Id")]
        public User User { get; set; }

        public byte[] Token { get; set; }

        public DateTime ExpirationTime { get; set; }
    }
}