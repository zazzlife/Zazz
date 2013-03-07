using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IUserFollowRequestRepository : IRepository<FollowRequest>
    {
        Task<int> GetReceivedRequestsCountAsync(int userId);

        Task<List<FollowRequest>> GetReceivedRequestsAsync(int userId);

        Task<List<FollowRequest>> GetSentRequestsAsync(int userId);

        Task RemoveAsync(int fromUserId, int toUserId);

        Task<bool> ExistsAsync(int fromUserId, int toUserId);
    }
}