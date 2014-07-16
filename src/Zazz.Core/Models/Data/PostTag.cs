using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    [Table("Post_Tags")]
    public class PostTag : BaseEntity
    {
        [Key, Column(Order = 0)]
        public int PostId { get; set; }

        [Key, Column(Order = 1)]
        public int ClubId { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }

        [ForeignKey("ClubId")]
        public virtual User Club { get; set; }
    }
}