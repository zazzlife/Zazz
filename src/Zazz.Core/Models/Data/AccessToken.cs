using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class AccessToken : BaseEntity
    {
        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("App")]
        public int AppId { get; set; }

        [MaxLength(4000)]
        public string Token { get; set; }

        public virtual User User { get; set; }

        public virtual App App { get; set; }

        public DateTime IssuedTime { get; set; }
    }
}