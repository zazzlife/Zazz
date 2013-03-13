using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class Photo : BaseEntity
    {
        [ForeignKey("UploaderId")]
        public virtual User Uploader { get; set; }

        public int UploaderId { get; set; }

        [MaxLength(250)]
        public string Description { get; set; }

        [ForeignKey("AlbumId")]
        public virtual Album Album { get; set; }

        public int AlbumId { get; set; }

        public DateTime UploadDate { get; set; }
    }
}