using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class EventFeed
    {
        [Key]
        public int FeedId { get; set; }

        public int EventId { get; set; }

        [ForeignKey("FeedId")]
        public virtual Feed Feed { get; set; }

        [ForeignKey("EventId")]
        public virtual ZazzEvent Event { get; set; }
    }
}