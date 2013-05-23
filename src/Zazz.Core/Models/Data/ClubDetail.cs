using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class ClubDetail : BaseEntity
    {
        [ForeignKey("Id")]
        public User User { get; set; }

        [MaxLength(500)]
        public string ClubName { get; set; }

        [ForeignKey("ClubType")]
        public byte ClubTypeId { get; set; }

        public virtual ClubType ClubType { get; set; }

        [MaxLength(500)]
        public string Address { get; set; }

        public int CoverPhotoId { get; set; }
    }
}