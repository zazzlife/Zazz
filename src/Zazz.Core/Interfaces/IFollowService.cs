using System;
using System.Threading.Tasks;

namespace Zazz.Core.Interfaces
{
    public interface IFollowService : IDisposable
    {
        Task FollowClubAsync(int userId,int clubId);

        Task SendFollowRequestAsync(int fromUserId, int toUserId);

        Task AcceptFollowRequestAsync(int requestId);

        Task RejectFollowRequestAsync(int requestId);
    }
}