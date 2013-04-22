using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    [Table("Feed_Photo")]
    public class FeedPhoto
    {
        [ForeignKey("FeedId")]
        public Feed Feed { get; set; }

        [Key]
        public int FeedId { get; set; }

        [ForeignKey("PhotoId")]
        public Photo Photo { get; set; }

        [Key]
        public int PhotoId { get; set; }
    }
}