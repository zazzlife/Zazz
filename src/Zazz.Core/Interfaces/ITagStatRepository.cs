using System;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface ITagStatRepository : IRepository<TagStat>
    {
        TagStat GetLastestTagStat(byte tagId);
    }
}