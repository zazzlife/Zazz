using System;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;

namespace Zazz.Infrastructure.Services
{
    public class VoteService : IVoteService
    {
        private readonly IUoW _uow;

        public VoteService(IUoW uow)
        {
            _uow = uow;
        }

        public void AddPhotoVote(int photoId, int currentUserId)
        {
            if (photoId == 0)
                throw new ArgumentOutOfRangeException("photoId");

            if (currentUserId == 0)
                throw new ArgumentOutOfRangeException("currentUserId");

            if (!_uow.PhotoRepository.Exists(photoId))
                throw new NotFoundException();
        }

        public void RemovePhotoVote(int photoId, int currentUserId)
        {
            throw new System.NotImplementedException();
        }
    }
}