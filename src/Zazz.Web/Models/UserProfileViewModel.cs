using System.Collections.Generic;
using Zazz.Core.Models;

namespace Zazz.Web.Models
{
    public class UserProfileViewModel
    {
        public PhotoLinks UserPhoto { get; set; }

        public string Major { get; set; }

        public string School { get; set; }

        public string City { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public int FollowersCount { get; set; }

        public int FollowingsCount { get; set; }

        public int ReceivedVotesCount { get; set; }

        public bool IsSelf { get; set; }

        public bool IsTargetUserFollowingCurrentUser { get; set; }

        public bool IsCurrentUserFollowingTargetUser { get; set; }

        public bool FollowRequestAlreadySent { get; set; }

        public List<FeedViewModel> Feeds { get; set; }

        public IEnumerable<CategoryStatViewModel> CategoriesStats { get; set; }

        public IEnumerable<PhotoViewModel> Photos { get; set; }
    }

}