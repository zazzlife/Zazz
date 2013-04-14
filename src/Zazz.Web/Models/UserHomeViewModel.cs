using System.Collections.Generic;
using Zazz.Core.Models.Data;

namespace Zazz.Web.Models
{
    public class UserHomeViewModel : BaseUserPageLayoutViewModel
    {
        public AccountType AccountType { get; set; }
        public IEnumerable<FeedViewModel> Feeds { get; set; }
    }
}