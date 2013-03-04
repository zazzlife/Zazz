using System;
using System.Collections.Generic;
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

        public async Task SendFollowRequestAsync(int fromUserId, int toUserId)
        {
            var exists = await _uow.UserFollowRequestRepository.ExistsAsync(fromUserId, toUserId);
            if (exists)
                return;

            var request = new UserFollowRequest
                              {
                                  FromUserId = fromUserId,
                                  ToUserId = toUserId,
                                  RequestDate = DateTime.UtcNow
                              };

            _uow.UserFollowRequestRepository.InsertGraph(request);
            await _uow.SaveAsync();
        }

        public async Task AcceptFollowRequestAsync(int requestId)
        {
            var followRequest = await _uow.UserFollowRequestRepository.GetByIdAsync(requestId);
            if (followRequest == null)
                return;

            var userFollow = new UserFollow { FromUserId = followRequest.FromUserId, ToUserId = followRequest.ToUserId };
            _uow.UserFollowRepository.InsertGraph(userFollow);
            _uow.UserFollowRequestRepository.Remove(followRequest);

            await _uow.SaveAsync();
        }

        public async Task RejectFollowRequestAsync(int requestId)
        {
            var request = await _uow.UserFollowRequestRepository.GetByIdAsync(requestId);
            if (request == null)
                return;

            _uow.UserFollowRequestRepository.Remove(request);
            await _uow.SaveAsync();
        }

        public Task<int> GetFollowRequestsCountAsync(int userId)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<UserFollowRequest>> GetFollowRequestsAsync(int userId)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}