using System.Collections.Generic;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;

namespace Zazz.Web.Models
{
    public class UserProfileViewModel
    {
        public string CoverPhotoUrl { get; set; }

        public PhotoLinks UserPhotoUrl { get; set; }

        public string Major { get; set; }

        public string School { get; set; }

        public string City { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public int FollowersCount { get; set; }

        public AccountType AccountType { get; set; }

        public bool IsSelf { get; set; }

        public bool IsTargetUserFollowingCurrentUser { get; set; }

        public bool IsCurrentUserFollowingTargetUser { get; set; }

        public bool FollowRequestAlreadySent { get; set; }

        public bool IsClub { get; set; }

        public List<FeedViewModel> Feeds { get; set; }
    }

}