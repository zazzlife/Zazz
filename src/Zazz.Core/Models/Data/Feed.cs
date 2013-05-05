using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Models.Data
{
    public class Feed : BaseEntity
    {
        public Feed()
        {
            FeedPhotos = new HashSet<FeedPhoto>();
            FeedUsers = new HashSet<FeedUser>();
        }

        public virtual PostFeed PostFeed { get; set; }

        public virtual EventFeed EventFeed { get; set; }

        public DateTime Time { get; set; }

        public FeedType FeedType { get; set; }

        public ICollection<FeedPhoto> FeedPhotos { get; set; }

        public ICollection<FeedUser> FeedUsers { get; set; }
    }
}