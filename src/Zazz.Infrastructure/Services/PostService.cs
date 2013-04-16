using System;
using System.Security;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class PostService : IPostService
    {
        private readonly IUoW _uow;

        public PostService(IUoW uow)
        {
            _uow = uow;
        }

        public void NewPost(Post post)
        {
            _uow.PostRepository.InsertGraph(post);

            var feed = new Feed
                       {
                           PostId = post.Id,
                           Time = post.CreatedTime,
                           UserId = post.FromUserId
                       };

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
                throw new SecurityException();
            
            _uow.PostRepository.Remove(post);
            _uow.FeedRepository.RemovePostFeeds(postId);
            _uow.CommentRepository.RemovePostComments(postId);

            _uow.SaveChanges();
        }
    }
}