using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class ClubImage : BaseEntity
    {
        [ForeignKey("ClubId")]
        public Club Club { get; set; }

        public int ClubId { get; set; }

        [MaxLength(250)]
        public string Description { get; set; }

        [ForeignKey("AlbumId")]
        public ClubAlbum Album { get; set; }

        public int AlbumId { get; set; }

        public DateTime UploadDate { get; set; }
    }
}