using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface ITagStatRepository : IRepository<TagStat>
    {
        TagStat GetTagStat(byte tagId);

        int GetUsersCount(int tagId);
    }
}