using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class Comment : BaseEntity
    {
        [ForeignKey("UserId")]
        public User User { get; set; }

        public int UserId { get; set; }

        public virtual PhotoComment PhotoComment { get; set; }

        public virtual PostComments PostComment { get; set; }

        [ForeignKey("EventId")]
        public virtual ZazzEvent Event { get; set; }

        public int? EventId { get; set; }

        [MaxLength(4000)]
        public string Message { get; set; }

        public DateTime Time { get; set; }
    }
}