using System.Collections.Generic;
using Zazz.Core.Models.Data;

namespace Zazz.Web.Models
{
    public class UserHomeViewModel
    {
        public AccountType AccountType { get; set; }
        public IEnumerable<FeedViewModel> Feeds { get; set; }
    }
}