using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    [Table("Posts_Tags")]
    public class PostTag
    {
        [Key, Column(Order = 0)]
        public byte TagId { get; set; }

        [Key, Column(Order = 1)]
        public int PostId { get; set; }

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }
    }
}