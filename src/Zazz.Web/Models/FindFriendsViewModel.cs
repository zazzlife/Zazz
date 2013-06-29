using System.Collections.Generic;
using Zazz.Core.Models;

namespace Zazz.Web.Models
{
    public class FindFriendsViewModel
    {
        public IEnumerable<FriendViewModel> Friends { get; set; }
    }

    public class FriendViewModel
    {
        public int UserId { get; set; }

        public string Name { get; set; }

        public PhotoLinks Photo { get; set; }
    }
}