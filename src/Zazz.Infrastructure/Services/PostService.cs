using System;
using System.Security;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class PostService : IPostService
    {
        private readonly IUoW _uow;
        private readonly INotificationService _notificationService;

        public PostService(IUoW uow, INotificationService notificationService)
        {
            _uow = uow;
            _notificationService = notificationService;
        }

        public void NewPost(Post post)
        {
            _uow.PostRepository.InsertGraph(post);

            var feed = new Feed
                       {
                           PostId = post.Id,
                           Time = post.CreatedTime,
                       };

            feed.FeedUsers.Add(new FeedUser
                                 {
                                     UserId = post.FromUserId
                                 });

            if (post.ToUserId.HasValue)
            {
                feed.FeedUsers.Add(new FeedUser
                                     {
                                         UserId = post.ToUserId.Value
                                     });
            }

            _uow.FeedRepository.InsertGraph(feed);
            _uow.SaveChanges();
        }

        public void EditPost(int postId, string newText, int currentUserId)
        {
            var post = _uow.PostRepository.GetById(postId);
            if (post == null)
                throw new Exception("Not Found");

            if (post.FromUserId != currentUserId)
                throw new SecurityException();

            post.Message = newText;

            _uow.SaveChanges();
        }

        public void RemovePost(int postId, int currentUserId)
        {
            var post = _uow.PostRepository.GetById(postId);
            if (post == null)
                return;

            if (post.FromUserId != currentUserId)
            {
                if (post.ToUserId.HasValue)
                {
                    if (post.ToUserId.Value != currentUserId)
                    {
                        throw new SecurityException();
                    }
                }
                else
                {
                    throw new SecurityException();
                }
            }
            
            _uow.PostRepository.Remove(post);
            _uow.FeedRepository.RemovePostFeeds(postId);
            _uow.CommentRepository.RemovePostComments(postId);
            _notificationService.RemovePostNotifications(postId);

            _uow.SaveChanges();
        }
    }
}