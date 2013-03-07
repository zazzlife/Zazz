using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class EventDetail : BaseEntity
    {
        [ForeignKey("Id")]
        public virtual Post Post { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [MaxLength(100)]
        public string Location { get; set; }
    }
}