using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Zazz.Core.Models.Data;

namespace Zazz.Web.Models
{
    public class EditClubProfileViewModel
    {
        [Display(Name = "Club Name")]
        public string ClubName { get; set; }

        [Display(Name = "Club Address")]
        public string ClubAddress { get; set; }

        public byte ClubType { get; set; }

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