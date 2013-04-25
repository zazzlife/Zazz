using System;
using System.Security;
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
            _uow.CommentRepository.InsertGraph(comment);
            _uow.SaveChanges();

            if (commentType == CommentType.Photo && comment.PhotoId.HasValue)
            {
                var photoId = comment.PhotoId.Value;
                var photo = _uow.PhotoRepository.GetById(photoId);

                if (photo.UserId != comment.FromId)
                    _notificationService.CreatePhotoCommentNotification(comment.Id, photoId, photo.UserId, save: false);
            }
            else if (commentType == CommentType.Post && comment.PostId.HasValue)
            {
                var postId = comment.PostId.Value;
                var post = _uow.PostRepository.GetById(postId);

                _notificationService.CreatePostCommentNotification(comment.Id, postId, post.FromUserId, save: false);
            }
            else if (commentType == CommentType.Event && comment.EventId.HasValue)
            {
                var eventId = comment.EventId.Value;
                var zazzEvent = _uow.EventRepository.GetById(eventId);

                _notificationService.CreateEventCommentNotification(comment.Id, eventId, zazzEvent.UserId, save: false);
            }

            _uow.SaveChanges();
            return comment.Id;
        }

        public void EditComment(int commentId, int currentUserId, string newComment)
        {
            var comment = _uow.CommentRepository.GetById(commentId);
            if (comment == null)
                return;

            if (comment.FromId != currentUserId)
                throw new SecurityException();

            comment.Message = newComment;

            _uow.SaveChanges();
        }

        public void RemoveComment(int commentId, int currentUserId)
        {
            var comment = _uow.CommentRepository.GetById(commentId);
            if (comment == null)
                return;

            if (comment.FromId != currentUserId)
                throw new SecurityException();

            _notificationService.RemoveCommentNotifications(commentId);
            _uow.CommentRepository.Remove(comment);

            _uow.SaveChanges();
        }

        public void RemovePhotoComments(int photoId)
        {
            var commentIds = _uow.CommentRepository.RemovePhotoComments(photoId);
            foreach (var commentId in commentIds)
            {
                _notificationService.RemoveCommentNotifications(commentId);
            }

            _uow.SaveChanges();
        }

        public void RemovePostComments(int postId)
        {
            var commentIds = _uow.CommentRepository.RemovePostComments(postId);
            foreach (var commentId in commentIds)
            {
                _notificationService.RemoveCommentNotifications(commentId);
            }

            _uow.SaveChanges();
        }

        public void RemoveEventComments(int eventId)
        {
            var commentIds = _uow.CommentRepository.RemoveEventComments(eventId);
            foreach (var commentId in commentIds)
            {
                _notificationService.RemoveCommentNotifications(commentId);
            }

            _uow.SaveChanges();
        }
    }
}