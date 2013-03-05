using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class ClubPost : BaseEntity
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

        public bool IsEvent { get; set; }

        public long FacebookObjectId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(100)]
        public string Location { get; set; }
    }
}