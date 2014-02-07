using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class ClubFollowersViewModel : ClubProfileViewModelBase
    {
        public IEnumerable<UserViewModel> Followers { get; set; }
    }
}