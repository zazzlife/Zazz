using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class UserFollowingClubsViewModel : UserProfileViewModelBase
    {
        public IEnumerable<ClubViewModel> Clubs { get; set; }
    }
}