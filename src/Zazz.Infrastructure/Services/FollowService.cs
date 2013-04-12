using System;
using System.Collections.Generic;
using System.Security;
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
            var exists = _uow.FollowRepository.Exists(fromUserId, clubAdminUserId);
            if (exists)
                return;

            var follow = new Follow { FromUserId = fromUserId, ToUserId = clubAdminUserId };
            _uow.FollowRepository.InsertGraph(follow);

            _uow.SaveChanges();
        }

        public async Task SendFollowRequestAsync(int fromUserId, int toUserId)
        {
            var exists = _uow.FollowRequestRepository.Exists(fromUserId, toUserId);
            if (exists)
                return;

            var request = new FollowRequest
                              {
                                  FromUserId = fromUserId,
                                  ToUserId = toUserId,
                                  RequestDate = DateTime.UtcNow
                              };

            _uow.FollowRequestRepository.InsertGraph(request);
            _uow.SaveChanges();
        }

        public async Task AcceptFollowRequestAsync(int requestId, int currentUserId)
        {
            var followRequest = _uow.FollowRequestRepository.GetById(requestId);
            if (followRequest == null)
                return;

            if (followRequest.ToUserId != currentUserId)
                throw new SecurityException();

            var userFollow = new Follow { FromUserId = followRequest.FromUserId, ToUserId = followRequest.ToUserId };
            _uow.FollowRepository.InsertGraph(userFollow);
            _uow.FollowRequestRepository.Remove((FollowRequest) followRequest);

            _uow.SaveChanges();
        }

        public async Task RejectFollowRequestAsync(int requestId, int currentUserId)
        {
            var request = _uow.FollowRequestRepository.GetById(requestId);
            if (request == null)
                return;

            if (request.ToUserId != currentUserId)
                throw new SecurityException();

            _uow.FollowRequestRepository.Remove((FollowRequest) request);
            _uow.SaveChanges();
        }

        public async Task RemoveFollowAsync(int fromUserId, int toUserId)
        {
            _uow.FollowRepository.Remove(fromUserId, toUserId);
            _uow.SaveChanges();
        }

        public async Task<int> GetFollowRequestsCountAsync(int userId)
        {
            return _uow.FollowRequestRepository.GetReceivedRequestsCount(userId);
        }

        public async Task<List<FollowRequest>> GetFollowRequestsAsync(int userId)
        {
            return _uow.FollowRequestRepository.GetReceivedRequests(userId);
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}