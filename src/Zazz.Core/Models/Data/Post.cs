using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class Post : BaseEntity
    {
        public Post()
        {
            Tags = new HashSet<Tag>();
        }

        [ForeignKey("FromUserId")]
        public User FromUser { get; set; }

        public int FromUserId { get; set; }

        [ForeignKey("ToUserId")]
        public User ToUser { get; set; }

        public int? ToUserId { get; set; }

        public string Message { get; set; }

        public long FacebookId { get; set; }
        
        [ForeignKey("PageId")]
        public FacebookPage Page { get; set; }

        public int? PageId { get; set; }

        public DateTime CreatedTime { get; set; }

        public virtual ICollection<Tag> Tags { get; set; }
    }
}