using System.Collections.Generic;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface ICategoryService
    {
        IEnumerable<CategoryStat> GetAllStats();
        void UpdateStatistics();
    }
}