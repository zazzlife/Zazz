using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IFollowService : IDisposable
    {
        Task FollowClubAdminAsync(int fromUserId, int clubAdminUserId);

        Task SendFollowRequestAsync(int fromUserId, int toUserId);

        Task AcceptFollowRequestAsync(int requestId, int currentUserId);

        Task RejectFollowRequestAsync(int requestId, int currentUserId);

        Task RemoveFollowAsync(int fromUserId, int toUserId);

        Task<int> GetFollowRequestsCountAsync(int userId);

        Task<List<FollowRequest>> GetFollowRequestsAsync(int userId);
    }
}