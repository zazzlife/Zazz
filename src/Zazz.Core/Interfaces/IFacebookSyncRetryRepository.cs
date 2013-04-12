using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
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