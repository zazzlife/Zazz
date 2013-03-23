using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class Feed : BaseEntity
    {
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public int UserId { get; set; }

        [ForeignKey("EventId")]
        public virtual ZazzEvent Event { get; set; }

        public int? EventId { get; set; }

        [ForeignKey("PhotoId")]
        public virtual Photo Photo { get; set; }

        public int? PhotoId { get; set; }

        public DateTime Time { get; set; }

        public FeedType FeedType { get; set; }
    }
}