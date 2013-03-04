using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IFollowService : IDisposable
    {
        Task FollowClubAsync(int userId,int clubId);

        Task SendFollowRequestAsync(int fromUserId, int toUserId);

        Task AcceptFollowRequestAsync(int requestId);

        Task RejectFollowRequestAsync(int requestId);

        Task<int> GetFollowRequestsCount(int userId);

        Task<IEnumerable<UserFollowRequest>> GetFollowRequests(int userId);
    }
}