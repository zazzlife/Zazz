using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class UserFollowingViewModel : UserProfileViewModelBase
    {
        public IEnumerable<UserViewModel> Follows { get; set; }
    }
}