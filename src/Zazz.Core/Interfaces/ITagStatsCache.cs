using System;
using System.Collections.Generic;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface ITagStatsCache
    {
        DateTime LastUpdate { get; set; }

        IList<TagStat> TagStats { get; set; }
    }
}