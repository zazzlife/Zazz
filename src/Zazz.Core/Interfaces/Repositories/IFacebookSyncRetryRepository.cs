using System.Collections.Generic;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface IFacebookSyncRetryRepository : IRepository<FacebookSyncRetry>
    {
        /// <summary>
        /// Gets 50 records that are the oldest
        /// </summary>
        /// <returns></returns>
        List<FacebookSyncRetry> GetEligibleRetries();
    }
}