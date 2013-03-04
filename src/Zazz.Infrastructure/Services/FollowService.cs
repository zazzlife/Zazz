using System.Threading.Tasks;
using Zazz.Core.Interfaces;

namespace Zazz.Infrastructure.Services
{
    public class FollowService : IFollowService
    {
        private readonly IUoW _uow;

        public FollowService(IUoW uow)
        {
            _uow = uow;
        }

        public Task FollowClubAsync(int userId, int clubId)
        {
            throw new System.NotImplementedException();
        }

        public Task SendFollowRequestAsync(int fromUserId, int toUserId)
        {
            throw new System.NotImplementedException();
        }

        public Task AcceptFollowRequestAsync(int requestId)
        {
            throw new System.NotImplementedException();
        }

        public Task RejectFollowRequestAsync(int requestId)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}