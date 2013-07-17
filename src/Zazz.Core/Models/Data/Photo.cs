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
            Tags = new HashSet<PhotoCategory>();
        }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public int UserId { get; set; }

        [MaxLength(4000)]
        public string Description { get; set; }

        [ForeignKey("AlbumId")]
        public virtual Album Album { get; set; }

        public int? AlbumId { get; set; }

        public bool IsFacebookPhoto { get; set; }

        [MaxLength(4000)]
        public string FacebookId { get; set; }

        public DateTime UploadDate { get; set; }

        [ForeignKey("PageId")]
        public FacebookPage Page { get; set; }

        public int? PageId { get; set; }

        public virtual ICollection<PhotoCategory> Tags { get; set; }
    }
}