using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class FeedPhotoId : BaseEntity
    {
        [ForeignKey("FeedId")]
        public Feed Feed { get; set; }

        public int FeedId { get; set; }

        [ForeignKey("PhotoId")]
        public Photo Photo { get; set; }

        public int PhotoId { get; set; }
    }
}