using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class Feed : BaseEntity
    {
        public Feed()
        {
            FeedPhotos = new HashSet<FeedPhoto>();
            FeedUsers = new HashSet<FeedUser>();
        }

        [ForeignKey("EventId")]
        public ZazzEvent Event { get; set; }

        public int? EventId { get; set; }

        [ForeignKey("PostId")]
        public Post Post { get; set; }

        public int? PostId { get; set; }

        public DateTime Time { get; set; }

        public FeedType FeedType { get; set; }

        public ICollection<FeedPhoto> FeedPhotos { get; set; }

        public ICollection<FeedUser> FeedUsers { get; set; }
    }
}