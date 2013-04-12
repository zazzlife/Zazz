using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IFollowRequestRepository : IRepository<FollowRequest>
    {
        int GetReceivedRequestsCount(int userId);

        List<FollowRequest> GetReceivedRequests(int userId);

        List<FollowRequest> GetSentRequests(int userId);

        void Remove(int fromUserId, int toUserId);

        bool Exists(int fromUserId, int toUserId);
    }
}