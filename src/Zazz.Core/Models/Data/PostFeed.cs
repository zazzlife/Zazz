using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class PostFeed
    {
        [Key]
        public int FeedId { get; set; }

        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }

        [ForeignKey("FeedId")]
        public virtual Feed Feed { get; set; }
    }
}