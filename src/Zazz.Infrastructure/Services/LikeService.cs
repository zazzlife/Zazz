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
        private readonly INotificationService _notificationService;

        public LikeService(IUoW uow,INotificationService notificationService)
        {
            _uow = uow;
            _notificationService = notificationService;
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
            if (currentUserId != photo.UserId)
                _notificationService.CreateLikePostNotification(currentUserId, photo.UserId, photoId);
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

        public int GetPostLikesCount(int postId)
        {
            if (postId == 0)
                throw new ArgumentOutOfRangeException("postId");

            return _uow.PostLikeRepository.GetLikesCount(postId);
        }

        public bool PostLikeExists(int postId, int userId)
        {
            if (postId == 0)
                throw new ArgumentOutOfRangeException("postId");

            if (userId == 0)
                throw new ArgumentOutOfRangeException("userId");

            

            return _uow.PostLikeRepository.Exists(postId, userId);
        }

        public void AddPostLike(int postId, int currentUserId)
        {
            if (postId == 0)
                throw new ArgumentOutOfRangeException("postId");

            if (currentUserId == 0)
                throw new ArgumentOutOfRangeException("currentUserId");

            var post = _uow.PostRepository.GetById(postId);
            if (post == null)
                throw new NotFoundException();

            if (_uow.PostLikeRepository.Exists(postId, currentUserId))
                throw new AlreadyLikedException();

            var like = new PostLike { PostId = postId, UserId = currentUserId };

            _uow.PostLikeRepository.InsertGraph(like);
            if (currentUserId != post.FromUserId)
                _notificationService.CreateLikePostNotification(currentUserId, post.FromUserId ,postId);
            _uow.UserReceivedLikesRepository.Increment(post.FromUserId);

            _uow.SaveChanges();
        }

        public void RemovePostLike(int postId, int currentUserId)
        {
            if (postId == 0)
                throw new ArgumentOutOfRangeException("postId");

            if (currentUserId == 0)
                throw new ArgumentOutOfRangeException("currentUserId");

            var post = _uow.PostRepository.GetById(postId);
            if (post == null)
                return;

            // This check is required because if we continue we'll decrement a like count, and if the current user hasn't liked we want to stop right here.
            if (!_uow.PostLikeRepository.Exists(postId, currentUserId))
                return;

            _uow.PostLikeRepository.Remove(postId, currentUserId);
            _uow.UserReceivedLikesRepository.Decrement(post.FromUserId);

            _uow.SaveChanges();
        }
    }
}