using System;
using System.Collections.Generic;
using System.Security;
using Zazz.Core.Exceptions;
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
            object obj;

            if (commentType == CommentType.Photo)
            {
                if (comment.PhotoComment == null || comment.PhotoComment.PhotoId == 0)
                    throw new Exception("comment type was photo, but photo was either null or 0");

                obj = _uow.PhotoRepository.GetById(comment.PhotoComment.PhotoId);
            }
            else if (commentType == CommentType.Post)
            {
                if (comment.PostComment == null || comment.PostComment.PostId == 0)
                    throw new Exception("comment type was post, but post was either null or 0");

                obj = _uow.PostRepository.GetById(comment.PostComment.PostId);
            }
            else if (commentType == CommentType.Event)
            {
                if (comment.EventComment == null || comment.EventComment.EventId == 0)
                    throw new Exception("comment type was event, but event was either null or 0");

                obj = _uow.EventRepository.GetById(comment.EventComment.EventId);
            }
            else
            {
                throw new ArgumentException("unknown comment type", "commentType");
            }

            if (obj == null)
                throw new NotFoundException();

            _uow.CommentRepository.InsertGraph(comment);
            _uow.SaveChanges();

            if (commentType == CommentType.Photo)
            {
                var photoId = comment.PhotoComment.PhotoId;
                var photo = (Photo) obj;

                if (photo.UserId != comment.UserId)
                    _notificationService.CreatePhotoCommentNotification(comment.Id, comment.UserId,
                                                                        photoId, photo.UserId, save: false);
            }
            else if (commentType == CommentType.Post)
            {
                var postId = comment.PostComment.PostId;
                var post = (Post)obj;

                // Creating a list of users to notify, at this moment maximum is 2 people but this way I leave a room to grow easily
                var usersToBeNotified = new List<int> { post.FromUserId };
                if (post.ToUserId.HasValue)
                    usersToBeNotified.Add(post.ToUserId.Value);

                // Removing the person that commented from notification list
                usersToBeNotified.Remove(comment.UserId);

                foreach (var userId in usersToBeNotified)
                {
                    _notificationService.CreatePostCommentNotification(comment.Id, comment.UserId, postId,
                                                                       userId, save: false);
                }
            }
            else if (commentType == CommentType.Event)
            {
                var eventId = comment.EventComment.EventId;
                var zazzEvent = (ZazzEvent) obj;

                if (comment.UserId != zazzEvent.UserId)
                {
                    _notificationService.CreateEventCommentNotification(comment.Id, comment.UserId, eventId,
                                                                        zazzEvent.UserId, save: false);
                }
            }

            _uow.SaveChanges();
            return comment.Id;
        }

        public void EditComment(int commentId, int currentUserId, string newComment)
        {
            var comment = _uow.CommentRepository.GetById(commentId);
            if (comment == null)
                throw new NotFoundException();

            if (comment.UserId != currentUserId)
                throw new SecurityException();

            comment.Message = newComment;

            _uow.SaveChanges();
        }

        public void RemoveComment(int commentId, int currentUserId)
        {
            var comment = _uow.CommentRepository.GetById(commentId);
            if (comment == null)
                return;

            if (comment.UserId != currentUserId)
                throw new SecurityException();

            _uow.CommentRepository.Remove(comment);
            _uow.SaveChanges();
        }
    }
}