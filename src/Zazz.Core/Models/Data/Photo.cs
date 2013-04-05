using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class Photo : BaseEntity
    {
        public Photo()
        {
            Comments = new HashSet<Comment>();
        }

        [ForeignKey("UploaderId")]
        public virtual User Uploader { get; set; }

        public int UploaderId { get; set; }

        [MaxLength(250)]
        public string Description { get; set; }

        [ForeignKey("AlbumId")]
        public virtual Album Album { get; set; }

        public int? AlbumId { get; set; }

        public bool IsFacebookPhoto { get; set; }

        [MaxLength(4000)]
        public string FacebookId { get; set; }

        [MaxLength(4000)]
        public string FacebookLink { get; set; }

        public DateTime UploadDate { get; set; }

        [ForeignKey("PageId")]
        public FacebookPage Page { get; set; }

        public int? PageId { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
    }
}