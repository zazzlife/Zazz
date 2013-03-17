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

        public async Task CreatePostAsync(Post post)
        {
            if (post.UserId == 0)
                throw new ArgumentException("User id cannot be 0");

            post.CreatedDate = DateTime.UtcNow;
            _uow.PostRepository.InsertGraph(post);

            await _uow.SaveAsync();

            var feed = new Feed
                       {
                           FeedType = post.IsEvent ? FeedType.Event : FeedType.Post,
                           PostId = post.Id,
                           UserId = post.UserId,
                           Time = post.CreatedDate
                       };

            _uow.FeedRepository.InsertGraph(feed);
            await _uow.SaveAsync();
        }

        public async Task UpdatePostAsync(Post post, int currentUserId)
        {
            if (post.Id == 0)
                throw new ArgumentException();

            var currentOwner = await _uow.PostRepository.GetOwnerIdAsync(post.Id);
            if (currentOwner != currentUserId)
                throw new SecurityException();

            // if you want to set update datetime later, the place would be here!
            _uow.PostRepository.InsertOrUpdate(post);
            await _uow.SaveAsync();
        }

        public Task<Post> GetPostAsync(int id)
        {
            return _uow.PostRepository.GetByIdAsync(id);
        }

        public async Task DeletePostAsync(int postId, int currentUserId)
        {
            if (postId == 0)
                throw new ArgumentException();

            var ownerId = await _uow.PostRepository.GetOwnerIdAsync(postId);
            if (ownerId != currentUserId)
                throw new SecurityException();

            _uow.FeedRepository.RemovePostFeed(postId);
            await _uow.PostRepository.RemoveAsync(postId);
            await _uow.SaveAsync();
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}