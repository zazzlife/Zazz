using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    [Table("Post_Categories")]
    public class PostCategory
    {
        [Key, Column(Order = 0)]
        public byte CategoryId { get; set; }

        [Key, Column(Order = 1)]
        public int PostId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }
    }
}