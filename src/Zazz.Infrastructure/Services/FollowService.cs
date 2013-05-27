﻿using System;
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
        private readonly INotificationService _notificationService;

        public FollowService(IUoW uow, INotificationService notificationService)
        {
            _uow = uow;
            _notificationService = notificationService;
        }

        public bool IsFollowing(int fromUser, int toUser)
        {
            return _uow.FollowRepository.Exists(fromUser, toUser);
        }

        public bool IsFollowRequestExists(int fromUser, int toUser)
        {
            return _uow.FollowRequestRepository.Exists(fromUser, toUser);
        }

        public void FollowClubAdmin(int fromUserId, int clubAdminUserId)
        {
            var exists = _uow.FollowRepository.Exists(fromUserId, clubAdminUserId);
            if (exists)
                return;

            var follow = new Follow { FromUserId = fromUserId, ToUserId = clubAdminUserId };
            _uow.FollowRepository.InsertGraph(follow);

            _uow.SaveChanges();
        }

        public void SendFollowRequest(int fromUserId, int toUserId)
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

        public void AcceptFollowRequest(int requestId, int currentUserId)
        {
            var followRequest = _uow.FollowRequestRepository.GetById(requestId);
            if (followRequest == null)
                return;

            if (followRequest.ToUserId != currentUserId)
                throw new SecurityException();

            var userFollow = new Follow { FromUserId = followRequest.FromUserId, ToUserId = followRequest.ToUserId };
            _uow.FollowRepository.InsertGraph(userFollow);
            _uow.FollowRequestRepository.Remove(followRequest);

            _notificationService.CreateFollowAcceptedNotification(followRequest.FromUserId,
                                                                  followRequest.ToUserId, false);

            _uow.SaveChanges();
        }

        public void RejectFollowRequest(int requestId, int currentUserId)
        {
            var request = _uow.FollowRequestRepository.GetById(requestId);
            if (request == null)
                return;

            if (request.ToUserId != currentUserId)
                throw new SecurityException();

            _uow.FollowRequestRepository.Remove((FollowRequest) request);
            _uow.SaveChanges();
        }

        public void RemoveFollow(int fromUserId, int toUserId)
        {
            _uow.FollowRepository.Remove(fromUserId, toUserId);
            _notificationService.RemoveFollowAcceptedNotification(fromUserId, toUserId, false);
            _uow.SaveChanges();
        }

        public int GetFollowRequestsCount(int userId)
        {
            return _uow.FollowRequestRepository.GetReceivedRequestsCount(userId);
        }

        public int GetFollowersCount(int userId)
        {
            return _uow.FollowRepository.GetFollowersCount(userId);
        }

        public List<FollowRequest> GetFollowRequests(int userId)
        {
            return _uow.FollowRequestRepository.GetReceivedRequests(userId);
        }
    }
}