using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public abstract class BaseImageEntity : BaseEntity
    {
        [ForeignKey("UploaderId")]
        public User Uploader { get; set; }

        public int UploaderId { get; set; }

        [MaxLength(250)]
        public string Description { get; set; }

        public DateTime UploadDate { get; set; }
    }
}