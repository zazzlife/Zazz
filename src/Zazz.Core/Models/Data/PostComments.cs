using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class PostComments
    {
        [Key]
        public int CommentId { get; set; }

        public int PostId { get; set; }

        [ForeignKey("CommentId")]
        public virtual Comment Comment { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; } 
    }
}