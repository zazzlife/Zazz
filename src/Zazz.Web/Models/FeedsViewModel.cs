using System;
using System.Collections.Generic;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Web.Models
{
    public class FeedsViewModel
    {
        public List<FeedViewModel> feeds { get; set; }

        public int remaining { get; set; }
    }
}