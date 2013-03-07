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

        [MaxLength(150)]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        [MaxLength(255)]
        public string PictureLink { get; set; }

        [MaxLength(255)]
        public string VideoLink { get; set; }

        public DateTime CreatedDate { get; set; }

        public long FacebookItemId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(100)]
        public string Location { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
    }
}