using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class UserPoint
    {
        [ForeignKey("User"), Key, Column(Order = 0)]
        public int UserId { get; set; }

        [ForeignKey("Club"), Key, Column(Order = 1)]
        public int ClubId { get; set; }

        public int Points { get; set; }
    }
}