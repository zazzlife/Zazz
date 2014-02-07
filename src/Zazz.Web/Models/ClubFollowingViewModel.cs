using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class ClubFollowingViewModel : ClubProfileViewModelBase
    {
        public IEnumerable<UserViewModel> Users { get; set; }
    }
}