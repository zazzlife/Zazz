namespace Zazz.Web.Models
{
    public class UserProfileViewModel
    {
        public string CoverPhotoUrl { get; set; }

        public string UserPhotoUrl { get; set; }

        public string Major { get; set; }

        public string University { get; set; }

        public string City { get; set; }

        public string UserName { get; set; }

        public int PartyWebCount { get; set; }

        public bool IsSelf { get; set; }

        public bool IsTargetUserFollowingCurrentUser { get; set; }

        public bool IsCurrentUserFollowingTargetUser { get; set; }
    }

}