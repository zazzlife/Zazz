using System.Collections.Generic;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface ITagService
    {
        IEnumerable<CategoryStat> GetAllTagStats();
        void UpdateTagStatistics();
    }
}