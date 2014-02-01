using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class UserProfileViewModel : UserProfileViewModelBase
    {
        public List<FeedViewModel> Feeds { get; set; }
         
    }
}