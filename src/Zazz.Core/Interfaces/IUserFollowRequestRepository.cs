using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IUserFollowRequestRepository : IRepository<UserFollowRequest>
    {
        Task<IList<UserFollowRequest>> GetReceivedRequests(int userId);

        Task<IList<UserFollowRequest>> GetSentRequests(int userId);

        Task Remove(int fromUserId, int toUserId);
    }
}