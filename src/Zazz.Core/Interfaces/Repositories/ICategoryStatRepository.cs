using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface ICategoryStatRepository : IRepository<CategoryStat>
    {
        CategoryStat GetById(byte categoryId);

        int GetUsersCount(int categoryId);
    }
}