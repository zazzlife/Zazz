using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    [Table("Events_Tags")]
    public class EventTag
    {
        [Key, Column(Order = 0)]
        public byte TagId { get; set; }

        [Key, Column(Order = 1)]
        public int EventId { get; set; }

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; }

        [ForeignKey("EventId")]
        public virtual ZazzEvent Event { get; set; } 
    }
}