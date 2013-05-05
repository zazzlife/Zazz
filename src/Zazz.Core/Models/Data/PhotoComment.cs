using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class PhotoComment
    {
        [Key]
        public int CommentId { get; set; }

        public int PhotoId { get; set; }

        [ForeignKey("CommentId")]
        public virtual Comment Comment { get; set; }

        [ForeignKey("PhotoId")]
        public virtual Photo Photo { get; set; }
    }
}