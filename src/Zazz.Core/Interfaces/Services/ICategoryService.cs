using System.Collections.Generic;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Services
{
    public interface ICategoryService
    {
        IEnumerable<CategoryStat> GetAllStats();
        void UpdateStatistics();
        IEnumerable<int> GetCategoryIds(IEnumerable<string> categoryNames);
    }
}