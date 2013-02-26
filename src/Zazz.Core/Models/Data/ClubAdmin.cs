using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class ClubAdmin : BaseEntity
    {
        [ForeignKey("UserId")]
        public User User { get; set; }

        public int UserId { get; set; }

        [ForeignKey("ClubId")]
        public Club Club { get; set; }

        public int ClubId { get; set; }

        /// <summary>
        /// User id of the person that has made this admin. There is no foreign key associated with this property.
        /// </summary>
        public int AssignedByUserId { get; set; }
    }
}