using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public abstract class ClubPostBase : EntityBase
    {
        [ForeignKey("ClubId")]
        public Club Club { get; set; }

        public int ClubId { get; set; }

        [MaxLength(150)]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        [MaxLength(255)]
        public string PictureLink { get; set; }

        [MaxLength(255)]
        public string VideoLink { get; set; }

        public DateTime CreatedDate { get; set; }

        [MaxLength(50)]
        public string FacebookItemId { get; set; }
    }
}