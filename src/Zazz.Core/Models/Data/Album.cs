using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class Album : BaseEntity
    {
        public Album()
        {
            Photos = new List<Photo>();
        }

        [MaxLength(50)]
        public string Name { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        public int UserId { get; set; }

        public bool IsFacebookAlbum { get; set; }

        [MaxLength(4000)]
        public string FacebookId { get; set; }

        [ForeignKey("PageId")]
        public FacebookPage Page { get; set; }

        public int? PageId { get; set; }

        public virtual ICollection<Photo> Photos { get; set; }

        [Column(TypeName = "DATE")]
        public DateTime CreatedDate { get; set; }
    }
}