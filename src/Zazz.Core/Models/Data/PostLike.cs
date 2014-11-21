using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class PostLike
    {
        [Key, Column(Order = 0), ForeignKey("Post")]
        public int PostId { get; set; }

        [Key, Column(Order = 1), ForeignKey("User")]
        public int UserId { get; set; }

        public virtual Post Post { get; set; }

        public virtual User User { get; set; }
    }
}
