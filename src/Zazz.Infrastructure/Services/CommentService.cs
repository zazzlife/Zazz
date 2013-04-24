using System;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Infrastructure.Services
{
    public class CommentService : ICommentService
    {
        private readonly IUoW _uow;
        private readonly INotificationService _notificationService;

        public CommentService(IUoW uow, INotificationService notificationService)
        {
            _uow = uow;
            _notificationService = notificationService;
        }

        public int CreateComment(Comment comment, CommentType commentType)
        {
            if (commentType == CommentType.Photo && comment.PhotoId.HasValue)
            {
                var photoId = comment.PhotoId.Value;
                var photo = _uow.PhotoRepository.GetById(photoId);

                _notificationService.CreatePhotoCommentNotification(photoId, photo.UserId, false);
            }
            else if (commentType == CommentType.Post && comment.PostId.HasValue)
            {
                var postId = comment.PostId.Value;
                var post = _uow.PostRepository.GetById(postId);
                
                _notificationService.CreatePostCommentNotification(postId, post.FromUserId, false);
            }

            _uow.CommentRepository.InsertGraph(comment);
            _uow.SaveChanges();

            return comment.Id;
        }

        public void EditComment(int commentId, int currentUserId, string newComment)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveComment(int commentId, int currentUserId)
        {
            throw new System.NotImplementedException();
        }
    }
}