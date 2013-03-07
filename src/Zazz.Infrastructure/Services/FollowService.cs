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

        public async Task FollowClubAdminAsync(int fromUserId, int clubAdminUserId)
        {
            var exists = await _uow.FollowRepository.ExistsAsync(fromUserId, clubAdminUserId);
            if (exists)
                return;

            var follow = new Follow { FromUserId = fromUserId, ToUserId = clubAdminUserId };
            _uow.FollowRepository.InsertGraph(follow);

            await _uow.SaveAsync();
        }

        public async Task SendFollowRequestAsync(int fromUserId, int toUserId)
        {
            var exists = await _uow.FollowRequestRepository.ExistsAsync(fromUserId, toUserId);
            if (exists)
                return;

            var request = new FollowRequest
                              {
                                  FromUserId = fromUserId,
                                  ToUserId = toUserId,
                                  RequestDate = DateTime.UtcNow
                              };

            _uow.FollowRequestRepository.InsertGraph(request);
            await _uow.SaveAsync();
        }

        public async Task AcceptFollowRequestAsync(int requestId)
        {
            var followRequest = await _uow.FollowRequestRepository.GetByIdAsync(requestId);
            if (followRequest == null)
                return;

            var userFollow = new Follow { FromUserId = followRequest.FromUserId, ToUserId = followRequest.ToUserId };
            _uow.FollowRepository.InsertGraph(userFollow);
            _uow.FollowRequestRepository.Remove(followRequest);

            await _uow.SaveAsync();
        }

        public async Task RejectFollowRequestAsync(int requestId)
        {
            var request = await _uow.FollowRequestRepository.GetByIdAsync(requestId);
            if (request == null)
                return;

            _uow.FollowRequestRepository.Remove(request);
            await _uow.SaveAsync();
        }

        public async Task RemoveFollowAsync(int fromUserId, int toUserId)
        {
            await _uow.FollowRepository.RemoveAsync(fromUserId, toUserId);
            await _uow.SaveAsync();
        }

        public Task<int> GetFollowRequestsCountAsync(int userId)
        {
            return _uow.FollowRequestRepository.GetReceivedRequestsCountAsync(userId);
        }

        public Task<List<FollowRequest>> GetFollowRequestsAsync(int userId)
        {
            return _uow.FollowRequestRepository.GetReceivedRequestsAsync(userId);
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}