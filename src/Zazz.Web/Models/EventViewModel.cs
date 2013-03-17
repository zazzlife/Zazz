using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Zazz.Web.Models
{
    public class EventViewModel
    {
        [Display(AutoGenerateField = false), ReadOnly(true)]
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        [Required, DataType(DataType.MultilineText)]
        public string Detail { get; set; }

        [Required, Display(Name = "Time")]
        public DateTime StartTime { get; set; }

        [StringLength(80)]
        public string Location { get; set; }

        [StringLength(80)]
        public string Street { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        [DataType(DataType.Currency)]
        public float? Price { get; set; }

        [Display(AutoGenerateField = false)]
        public DateTime? CreatedDate { get; set; }

        [Display(AutoGenerateField = false)]
        public string FacebookLink { get; set; }

        [Display(AutoGenerateField = false)]
        public bool IsOwner { get; set; }

        [HiddenInput(DisplayValue = false)]
        public float? Latitude { get; set; }

        [HiddenInput(DisplayValue = false)]
        public float? Longitude { get; set; }

        [Display(AutoGenerateField = false)]
        public string ImageUrl { get; set; }
    }
}