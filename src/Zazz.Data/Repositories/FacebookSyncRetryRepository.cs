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

        protected override int GetItemId(FacebookSyncRetry item)
        {
            throw new InvalidOperationException("You need to provide the id for updating the record. If you want to insert, use insert graph");
        }

        public List<FacebookSyncRetry> GetEligibleRetries()
        {
            return DbSet.OrderBy(r => r.LastTry)
                        .Take(50).ToList();
        }
    }
}