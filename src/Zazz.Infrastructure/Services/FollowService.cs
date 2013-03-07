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

        public async Task SendFollowRequestAsync(int fromUserId, int toUserId)
        {
            var exists = await _uow.UserFollowRequestRepository.ExistsAsync(fromUserId, toUserId);
            if (exists)
                return;

            var request = new FollowRequest
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

            var userFollow = new Follow { FromUserId = followRequest.FromUserId, ToUserId = followRequest.ToUserId };
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
            return _uow.UserFollowRequestRepository.GetReceivedRequestsCountAsync(userId);
        }

        public Task<List<FollowRequest>> GetFollowRequestsAsync(int userId)
        {
            return _uow.UserFollowRequestRepository.GetReceivedRequestsAsync(userId);
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}