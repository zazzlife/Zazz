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
    }
}