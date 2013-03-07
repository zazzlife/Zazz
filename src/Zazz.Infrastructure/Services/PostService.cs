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

        public async Task CreateEventAsync(Post post)
        {
            if (post.UserId == 0)
                throw new ArgumentException("User id cannot be 0");

            post.CreatedDate = DateTime.UtcNow;
            _uow.PostRepository.InsertGraph(post);
            await _uow.SaveAsync();
        }

        public async Task UpdateEventAsync(Post post, int currentUserId)
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

        public async Task DeleteEventAsync(int postId, int currentUserId)
        {
            if (postId == 0)
                throw new ArgumentException();

            var ownerId = await _uow.PostRepository.GetOwnerIdAsync(postId);
            if (ownerId != currentUserId)
                throw new SecurityException();

            await _uow.PostRepository.RemoveAsync(postId);
            await _uow.SaveAsync();
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}