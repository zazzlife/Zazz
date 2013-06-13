using System;
using System.Collections.Generic;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models
{
    public class UserHomeViewModel
    {
        public bool HasFacebookAccount { get; set; }

        public AccountType AccountType { get; set; }
        
        public IEnumerable<FeedViewModel> Feeds { get; set; }
    }
}