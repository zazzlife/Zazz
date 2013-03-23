using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class Comment : BaseEntity
    {
        [ForeignKey("FromId")]
        public User From { get; set; }

        public int FromId { get; set; }

        [ForeignKey("EventId")]
        public ZazzEvent Event { get; set; }

        public int? EventId { get; set; }

        [ForeignKey("PhotoId")]
        public Photo Photo { get; set; }

        public int? PhotoId { get; set; }

        [MaxLength(1000)]
        public string Message { get; set; }

        public DateTime Time { get; set; }
    }
}