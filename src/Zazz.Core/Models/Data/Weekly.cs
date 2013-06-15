using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Core.Models.Data
{
    public class Weekly : BaseEntity
    {
        [ForeignKey("User")]
        public int UserId { get; set; }

        public DayOfTheWeek DayOfTheWeek { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        [ForeignKey("Photo")]
        public int? PhotoId { get; set; }

        [MaxLength(4000)]
        public string Description { get; set; }

        public User User { get; set; }

        public Photo Photo { get; set; }
    }
}