using System;
using System.ComponentModel.DataAnnotations;

namespace Zazz.Web.Models
{
    public class EventViewModel
    {
        [Required, StringLength(150)]
        public string Name { get; set; }

        [Required, DataType(DataType.MultilineText)]
        public string Detail { get; set; }

        [Required, Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Display(Name = "End Time")]
        public DateTime? EndTime { get; set; }

        [StringLength(80)]
        public string Location { get; set; }

        [StringLength(80)]
        public string Street { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        [DataType(DataType.Currency)]
        public float? Price { get; set; }

        [Display(AutoGenerateField = false)]
        public DateTime CreatedDate { get; set; }
    }
}