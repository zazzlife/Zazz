using Zazz.Core.Models;

namespace Zazz.Web.Models
{
    public class ClubViewModel
    {
        public int ClubId { get; set; }

        public string ClubName { get; set; }

        public PhotoLinks ProfileImageLink { get; set; }

        public PhotoLinks CoverImageLink { get; set; }

        public bool IsCurrentUserFollowing { get; set; }

        public int CurrentUserId { get; set; }

        public string clubtypes { get; set; }
    }
}