using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Zazz.Web.Models
{
    public class EditProfileViewModel
    {
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        public SelectList Schools { get; set; }

        public int SchoolId { get; set; }

        public SelectList Majors { get; set; }

        public int MajorId { get; set; }

        public SelectList Cities { get; set; }

        public int CityId { get; set; }

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