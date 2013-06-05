using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IFollowService
    {
        bool IsFollowing(int fromUser, int toUser);

        bool IsFollowRequestExists(int fromUser, int toUser);

        void Follow(int fromUserId, int toUserId);

        void AcceptFollowRequest(int requestId, int currentUserId);

        void RejectFollowRequest(int requestId, int currentUserId);

        void RemoveFollow(int fromUserId, int toUserId);

        int GetFollowRequestsCount(int userId);

        int GetFollowersCount(int userId);

        List<FollowRequest> GetFollowRequests(int userId);

        IQueryable<Follow> GetFollowers(int userId);
    }
}