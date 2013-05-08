using System.Collections.Generic;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface ITagService
    {
        IEnumerable<TagStat> GetAllTagStats();
        void UpdateTagStatistics();
    }
}