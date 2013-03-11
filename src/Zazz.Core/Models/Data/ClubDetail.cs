using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class ClubDetail : BaseEntity
    {
        [ForeignKey("Id")]
        public User User { get; set; }

        [MaxLength(30)]
        public string ClubName { get; set; }
    }
}