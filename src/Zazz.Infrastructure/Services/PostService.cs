using System;
using System.Security;
using System.Threading.Tasks;
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

        public async Task NewPostAsync(Post post)
        {
            _uow.PostRepository.InsertGraph(post);

            var feed = new Feed
                       {
                           PostId = post.Id,
                           Time = post.CreatedTime,
                           UserId = post.UserId
                       };

            _uow.FeedRepository.InsertGraph(feed);

            _uow.SaveChanges();
        }

        public async Task EditPostAsync(int postId, string newText, int currentUserId)
        {
            var post = await _uow.PostRepository.GetByIdAsync(postId);
            if (post == null)
                throw new Exception("Not Found");

            if (post.UserId != currentUserId)
                throw new SecurityException();

            post.Message = newText;

            _uow.SaveChanges();
        }

        public async Task RemovePostAsync(int postId, int currentUserId)
        {
            var post = await _uow.PostRepository.GetByIdAsync(postId);
            if (post == null)
                return;

            if (post.UserId != currentUserId)
                throw new SecurityException();
            
            _uow.PostRepository.Remove(post);
            _uow.FeedRepository.RemovePostFeeds(postId);
            _uow.CommentRepository.RemovePostComments(postId);

            _uow.SaveChanges();
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}