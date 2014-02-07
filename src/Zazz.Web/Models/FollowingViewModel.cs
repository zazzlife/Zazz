using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class FollowingViewModel
    {
        public IEnumerable<UserViewModel> Users { get; set; }
    }
}