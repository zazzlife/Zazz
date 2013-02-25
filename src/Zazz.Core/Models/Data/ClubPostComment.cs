using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class ClubPostComment : BaseEntity
    {
        [ForeignKey("FromId")]
        public User From { get; set; }

        public int FromId { get; set; }

        [ForeignKey("PostId")]
        public ClubPost Post { get; set; }

        public int PostId { get; set; }

        [MaxLength(1000)]
        public string Message { get; set; }
    }
}