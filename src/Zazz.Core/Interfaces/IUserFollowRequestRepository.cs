using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IUserFollowRequestRepository : IRepository<UserFollowRequest>
    {
        Task<List<UserFollowRequest>> GetReceivedRequestsAsync(int userId);

        Task<List<UserFollowRequest>> GetSentRequestsAsync(int userId);

        Task RemoveAsync(int fromUserId, int toUserId);

        Task<bool> ExistsAsync(int fromUserId, int toUserId);
    }
}