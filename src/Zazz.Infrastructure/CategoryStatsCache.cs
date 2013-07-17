using System;
using System.Collections.Generic;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure
{
    //This class is a singleton
    public class CategoryStatsCache : ICategoryStatsCache
    {
        public DateTime LastUpdate { get; set; }
        public IList<CategoryStat> CategoryStats { get; set; }
    }
}