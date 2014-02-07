using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class FollowersViewModel
    {
        public IEnumerable<UserViewModel> FollowRequests { get; set; }
        public IEnumerable<UserViewModel> Followers { get; set; }
    }
}