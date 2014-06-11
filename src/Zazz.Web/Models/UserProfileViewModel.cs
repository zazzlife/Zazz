using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class UserProfileViewModel : UserProfileViewModelBase
    {
        public FeedsViewModel Feeds { get; set; }
    }
}