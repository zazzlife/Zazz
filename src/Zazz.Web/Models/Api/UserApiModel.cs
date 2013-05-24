using System.Collections.Generic;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models.Api
{
    public class ApiUser
    {
        public int Id { get; set; }

        public string DisplayName { get; set; }

        public PhotoLinks DisplayPhoto { get; set; }

        public AccountType AccountType { get; set; }

        public int FollowersCount { get; set; }

        public bool IsSelf { get; set; }

        public bool IsTargetUserFollowingCurrentUser { get; set; }

        public bool IsCurrentUserFollowingTargetUser { get; set; }

        public bool FollowRequestAlreadySent { get; set; }

        public IEnumerable<FeedApiResponse> Feeds { get; set; }

        public IEnumerable<PhotoApiModel> Photos { get; set; }

        public ApiUserPreferences Preferences { get; set; }
    }

    public class ApiUserDetails
    {
        public City City { get; set; }

        public Major Major { get; set; }

        public School School { get; set; }

        public string FullName { get; set; }

        public Gender Gender { get; set; }
    }

    public class ApiClubDetails
    {
        public ClubType ClubType { get; set; }

        public string Address { get; set; }

        public string ClubName { get; set; }

        public PhotoLinks CoverPhoto { get; set; }
    }

    public class ApiUserPreferences
    {
        public bool SyncFbEvents { get; set; }

        public bool SyncFbPosts { get; set; }

        public bool SyncFbImages { get; set; }

        public bool SendSyncErrorNotifications { get; set; } 
    }
}