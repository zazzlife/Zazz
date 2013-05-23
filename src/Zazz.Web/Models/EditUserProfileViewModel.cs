using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models
{
    public class EditUserProfileViewModel : BaseUserPageLayoutViewModel
    {
        public Gender Gender { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        public SelectList Schools { get; set; }

        public int? SchoolId { get; set; }

        public SelectList Majors { get; set; }

        public int? MajorId { get; set; }

        public SelectList Cities { get; set; }

        public int? CityId { get; set; }

        [Display(Name = "Sync Facebook Events")]
        public bool SyncFbEvents { get; set; }

        [Display(Name = "Email Facebook Errors Notification")]
        public bool SendFbErrorNotification { get; set; }
    }

    public class EditClubProfileViewModel : BaseUserPageLayoutViewModel
    {
        [Display(Name = "Club Name")]
        public string ClubName { get; set; }

        [Display(Name = "Club Address")]
        public string ClubAddress { get; set; }

        public int ClubType { get; set; }

        public IEnumerable<ClubType> ClubTypes { get; set; }

        [Display(Name = "Sync Facebook Events")]
        public bool SyncFbEvents { get; set; }

        [Display(Name = "Sync Facebook Posts")]
        public bool SyncFbPosts { get; set; }

        [Display(Name = "Sync Facebook Images")]
        public bool SyncFbImages { get; set; }

        [Display(Name = "Email Facebook Errors Notification")]
        public bool SendFbErrorNotification { get; set; }
    }
}