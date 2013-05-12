using System;
using System.Data.Entity;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class WeeklyRepository : BaseRepository<Weekly>, IWeeklyRepository
    {
        public WeeklyRepository(DbContext dbContext) : base(dbContext)
        {}

        protected override int GetItemId(Weekly item)
        {
            throw new InvalidOperationException("You must provide the Id for updating. Use InsertGraph for insert");
        }
    }
}