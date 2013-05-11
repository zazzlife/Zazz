using System;
using System.Collections.Generic;

namespace Zazz.Web.Models
{
    public class TagStatsWidgetViewModel
    {
        public IEnumerable<TagStatViewModel> Tags { get; set; }

        public DateTime LastUpdate { get; set; } 
    }
}