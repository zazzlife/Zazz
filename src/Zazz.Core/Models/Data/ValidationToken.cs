using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class ValidationToken : BaseEntity
    {
        [ForeignKey("Id")]
        public User User { get; set; }

        public int Token { get; set; }

        public DateTime ExpirationDate { get; set; }
    }
}