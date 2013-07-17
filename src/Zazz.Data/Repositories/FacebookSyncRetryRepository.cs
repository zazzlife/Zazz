using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class FacebookSyncRetryRepository : BaseRepository<FacebookSyncRetry>, IFacebookSyncRetryRepository
    {
        public FacebookSyncRetryRepository(DbContext dbContext) : base(dbContext)
        {}

        public List<FacebookSyncRetry> GetEligibleRetries()
        {
            return DbSet.OrderBy(r => r.LastTry)
                        .Take(50).ToList();
        }
    }
}