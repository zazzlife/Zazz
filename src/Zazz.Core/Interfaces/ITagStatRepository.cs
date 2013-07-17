using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface ITagStatRepository : IRepository<CategoryStat>
    {
        CategoryStat GetTagStat(byte tagId);

        int GetUsersCount(int tagId);
    }
}