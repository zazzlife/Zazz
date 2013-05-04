using System;
using System.Data.Entity;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class TagStatRepository : BaseRepository<TagStat>, ITagStatRepository
    {
        public TagStatRepository(DbContext dbContext) : base(dbContext)
        {}

        protected override int GetItemId(TagStat item)
        {
            throw new NotImplementedException();
        }

        public TagStat GetByDate(DateTime date)
        {
            throw new NotImplementedException();
        }
    }
}