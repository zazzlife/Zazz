using System;
using System.Security;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class UserEventService : IUserEventService
    {
        private readonly IUoW _uow;

        public UserEventService(IUoW uow)
        {
            _uow = uow;
        }

        public async Task CreateEventAsync(Post post)
        {
            if (post.UserId == 0)
                throw new ArgumentException("User id cannot be 0");

            post.CreatedDate = DateTime.UtcNow;
            _uow.UserEventRepository.InsertGraph(post);
            await _uow.SaveAsync();
        }

        public async Task UpdateEventAsync(Post post, int currentUserId)
        {
            if (post.Id == 0)
                throw new ArgumentException();

            var currentOwner = await _uow.UserEventRepository.GetOwnerIdAsync(post.Id);
            if (currentOwner != currentUserId)
                throw new SecurityException();

            // if you want to set update datetime later, the place would be here!
            _uow.UserEventRepository.InsertOrUpdate(post);
            await _uow.SaveAsync();
        }

        public async Task DeleteEventAsync(int userEventId, int currentUserId)
        {
            if (userEventId == 0)
                throw new ArgumentException();

            var ownerId = await _uow.UserEventRepository.GetOwnerIdAsync(userEventId);
            if (ownerId != currentUserId)
                throw new SecurityException();

            await _uow.UserEventRepository.RemoveAsync(userEventId);
            await _uow.SaveAsync();
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}