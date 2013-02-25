using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class UserEventComment : BaseEntity
    {
        [ForeignKey("FromId")]
        public User From { get; set; }

        public int FromId { get; set; }

        [ForeignKey("UserEventId")]
        public UserEvent UserEvent { get; set; }

        public int UserEventId { get; set; }

        [MaxLength(1000)]
        public string Message { get; set; } 
    }
}