using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class Post : BaseEntity
    {
        [ForeignKey("UserId")]
        public User User { get; set; }

        public int UserId { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public virtual ICollection<Link> Links { get; set; }

        public DateTime CreatedDate { get; set; }

        [MaxLength(100), DataType("varchar")]
        public string FacebookItemId { get; set; }

        [MaxLength(300), DataType(DataType.Url)]
        public string FacebookLink { get; set; }

        [MaxLength(300), DataType(DataType.ImageUrl)]
        public string FacebookPhotoLink { get; set; }

        public int? PhotoId { get; set; }

        public bool IsEvent { get; set; }

        public virtual EventDetail EventDetail { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
    }
}