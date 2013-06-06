using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class PhotoVote
    {
        [Key, Column(Order = 0), ForeignKey("Photo")]
        public int PhotoId { get; set; }

        [Key, Column(Order = 1), ForeignKey("User")]
        public int UserId { get; set; }

        public virtual Photo Photo { get; set; }

        public virtual User User { get; set; }
    }
}