using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models
{
    public class EditUserProfileViewModel
    {
        public Gender Gender { get; set; }

        [StringLength(30), Display(Name = "Full name")]
        [RegularExpression(@"^([a-zA-Z0-9 ._']+)$", ErrorMessage = "{0} contains invalid character(s)")]
        public string FullName { get; set; }

        public IEnumerable<School> Schools { get; set; }

        public short? SchoolId { get; set; }

        public IEnumerable<Major> Majors { get; set; }

        public byte? MajorId { get; set; }

        public IEnumerable<City> Cities { get; set; }

        public int? CityId { get; set; }

        [Display(Name = "Sync Facebook Events")]
        public bool SyncFbEvents { get; set; }

        [Display(Name = "Email Facebook Errors Notification")]
        public bool SendFbErrorNotification { get; set; }
    }
}