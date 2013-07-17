using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    [Table("Event_Categories")]
    public class EventCategory
    {
        [Key, Column(Order = 0)]
        public byte CategoryId { get; set; }

        [Key, Column(Order = 1)]
        public int EventId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [ForeignKey("EventId")]
        public virtual ZazzEvent Event { get; set; } 
    }
}