using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    [Table("Feed_Photos")]
    public class FeedPhoto
    {
        [Key, Column(Order = 0)]
        public int FeedId { get; set; }

        [Key, Column(Order = 1)]
        public int PhotoId { get; set; }

        [ForeignKey("FeedId")]
        public virtual Feed Feed { get; set; }

        [ForeignKey("PhotoId")]
        public virtual Photo Photo { get; set; }

        public string TagUser { get; set; }
        
    }
}