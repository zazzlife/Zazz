using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Services
{
    public interface IFollowService
    {
        bool IsFollowing(int fromUser, int toUser);

        bool IsFollowRequestExists(int fromUser, int toUser);

        void Follow(int fromUserId, int toUserId);

        void AcceptFollowRequest(int fromUserId, int currentUserId);

        void RejectFollowRequest(int fromUserId, int currentUserId);

        void RemoveFollow(int fromUserId, int toUserId);

        int GetFollowRequestsCount(int userId);

        int GetFollowersCount(int userId);

        IQueryable<FollowRequest> GetFollowRequests(int userId);

        IQueryable<Follow> GetFollowers(int userId);

        IQueryable<Follow> GetFollows(int userId);
    }
}