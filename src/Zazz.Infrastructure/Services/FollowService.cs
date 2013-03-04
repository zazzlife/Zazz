using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class FollowService : IFollowService
    {
        private readonly IUoW _uow;

        public FollowService(IUoW uow)
        {
            _uow = uow;
        }

        public async Task FollowClubAsync(int userId, int clubId)
        {
            var exists = await _uow.ClubFollowRepository.ExistsAsync(userId, clubId);
            if (exists)
                return;

            var follow = new ClubFollow { ClubId = clubId, UserId = userId };
            _uow.ClubFollowRepository.InsertGraph(follow); ;
            await _uow.SaveAsync();
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