using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class ClubProfileViewModel : ClubProfileViewModelBase
    {
        public List<FeedViewModel> Feeds { get; set; }
    }
}