using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class ClubAlbum : BaseEntity
    {
        [MaxLength(50)]
        public string Name { get; set; }

        [ForeignKey("ClubId")]
        public Club Club { get; set; }

        public int ClubId { get; set; }

        public virtual ICollection<ClubImage> Images { get; set; }
    }
}