using System;
using System.Collections.Generic;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface ICategoryStatsCache
    {
        DateTime LastUpdate { get; set; }

        IList<CategoryStat> CategoryStats { get; set; }
    }
}