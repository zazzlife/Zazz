using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class EventDetail : BaseEntity
    {
        [ForeignKey("Id")]
        public virtual Post Post { get; set; }

        public DateTime StartTime { get; set; }

        [MaxLength(80)]
        public string Location { get; set; }

        [MaxLength(80)]
        public string Street { get; set; }

        [MaxLength(50)]
        public string City { get; set; }

        public float Latitude { get; set; }

        public float Longitude { get; set; }

        [DataType(DataType.Currency)]
        public float? Price { get; set; }
    }
}