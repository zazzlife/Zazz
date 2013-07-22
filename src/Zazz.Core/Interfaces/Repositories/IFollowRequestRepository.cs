using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface IFollowRequestRepository
    {
        FollowRequest GetFollowRequest(int fromUserId, int toUserId);

        void InsertGraph(FollowRequest followRequest);

        int GetReceivedRequestsCount(int userId);

        IQueryable<FollowRequest> GetReceivedRequests(int userId);

        IQueryable<FollowRequest> GetSentRequests(int userId);

        void Remove(FollowRequest followRequest);
        
        void Remove(int fromUserId, int toUserId);

        bool Exists(int fromUserId, int toUserId);
    }
}