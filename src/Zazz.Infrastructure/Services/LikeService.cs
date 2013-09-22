using System;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class LikeService : ILikeService
    {
        private readonly IUoW _uow;

        public LikeService(IUoW uow)
        {
            _uow = uow;
        }

        public int GetPhotoLikesCount(int photoId)
        {
            if (photoId == 0)
                throw new ArgumentOutOfRangeException("photoId");

            return _uow.PhotoLikeRepository.GetLikesCount(photoId);
        }

        public bool PhotoLikeExists(int photoId, int userId)
        {
            if (photoId == 0)
                throw new ArgumentOutOfRangeException("photoId");

            if (userId == 0)
                throw new ArgumentOutOfRangeException("userId");

            return _uow.PhotoLikeRepository.Exists(photoId, userId);
        }

        public void AddPhotoLike(int photoId, int currentUserId)
        {
            if (photoId == 0)
                throw new ArgumentOutOfRangeException("photoId");

            if (currentUserId == 0)
                throw new ArgumentOutOfRangeException("currentUserId");

            var photo = _uow.PhotoRepository.GetPhotoWithMinimalData(photoId);
            if (photo == null)
                throw new NotFoundException();

            if (_uow.PhotoLikeRepository.Exists(photoId, currentUserId))
                throw new AlreadyLikedException();

            var like = new PhotoLike { PhotoId = photoId, UserId = currentUserId };
            
            _uow.PhotoLikeRepository.InsertGraph(like);
            _uow.UserReceivedLikesRepository.Increment(photo.UserId);

            _uow.SaveChanges();
        }

        public void RemovePhotoLike(int photoId, int currentUserId)
        {
            if (photoId == 0)
                throw new ArgumentOutOfRangeException("photoId");

            if (currentUserId == 0)
                throw new ArgumentOutOfRangeException("currentUserId");

            var photo = _uow.PhotoRepository.GetPhotoWithMinimalData(photoId);
            if (photo == null)
                return;

            // This check is required because if we continue we'll decrement a like count, and if the current user hasn't liked we want to stop right here.
            if (!_uow.PhotoLikeRepository.Exists(photoId, currentUserId))
                return;

            _uow.PhotoLikeRepository.Remove(photoId, currentUserId);
            _uow.UserReceivedLikesRepository.Decrement(photo.UserId);

            _uow.SaveChanges();
        }
    }
}