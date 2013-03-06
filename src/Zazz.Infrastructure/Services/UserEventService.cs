using System;
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

        public async Task CreateEventAsync(UserEvent userEvent)
        {
            if (userEvent.UserId == 0)
                throw new ArgumentException("User id cannot be 0");

            userEvent.CreatedDate = DateTime.UtcNow;
            _uow.UserEventRepository.InsertGraph(userEvent);
            await _uow.SaveAsync();
        }

        public Task UpdateEventAsync(UserEvent userEvent, int currentUserId)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteEventAsync(int userEventId, int currentUserId)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}