using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models
{
    public class EditClubProfileViewModel
    {
        [Required, StringLength(30), Display(Name = "Club Name")]
        [RegularExpression(@"^([a-zA-Z0-9 ._']+)$", ErrorMessage = "{0} contains invalid character(s)")]
        public string ClubName { get; set; }

        [Display(Name = "Club Address")]
        public string ClubAddress { get; set; }

        public IEnumerable<ClubType> ClubTypes { get; set; }

        [Display(Name = "Sync Facebook Events")]
        public bool SyncFbEvents { get; set; }

        [Display(Name = "Sync Facebook Posts")]
        public bool SyncFbPosts { get; set; }

        [Display(Name = "Sync Facebook Images")]
        public bool SyncFbImages { get; set; }

        [Display(Name = "Email Facebook Errors Notification")]
        public bool SendFbErrorNotification { get; set; }

        [Display(Name = "School")]
        public short? SchoolId { get; set; }

        [Display(Name = "City")]
        public int? CityId { get; set; }

        public IEnumerable<School> Schools { get; set; }

        public IEnumerable<City> Cities { get; set; }
    }
}