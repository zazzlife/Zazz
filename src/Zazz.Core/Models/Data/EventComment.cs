using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class EventComment
    {
        [Key]
        public int CommentId { get; set; }

        public int EventId { get; set; }

        [ForeignKey("CommentId")]
        public Comment Comment { get; set; }

        [ForeignKey("EventId")]
        public ZazzEvent Event { get; set; }
    }
}