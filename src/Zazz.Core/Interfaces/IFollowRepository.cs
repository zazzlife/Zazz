using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IFollowRepository : IRepository<Follow>
    {
        /// <summary>
        /// Returns a list of users that follow the user
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Follow>> GetUserFollowersAsync(int toUserId);

        /// <summary>
        /// Returns a list of users that the user follows
        /// </summary>
        /// <param name="fromUserId"></param>
        /// <returns></returns>
        Task<IEnumerable<Follow>> GetUserFollowsAsync(int fromUserId);

        Task<bool> ExistsAsync(int fromUserId, int toUserId);

        Task RemoveAsync(int fromUserId, int toUserId);
    }
}