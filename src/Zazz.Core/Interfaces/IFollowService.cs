using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IFollowService : IDisposable
    {
        void FollowClubAdmin(int fromUserId, int clubAdminUserId);

        void SendFollowRequest(int fromUserId, int toUserId);

        void AcceptFollowRequest(int requestId, int currentUserId);

        void RejectFollowRequest(int requestId, int currentUserId);

        void RemoveFollow(int fromUserId, int toUserId);

        int GetFollowRequestsCount(int userId);

        List<FollowRequest> GetFollowRequests(int userId);
    }
}