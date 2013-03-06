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

        public Task CreateEventAsync(UserEvent userEvent)
        {
            throw new System.NotImplementedException();
        }

        public Task UpdateEventAsync(UserEvent userEvent, int currentUserId)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteEventAsync(int userEventId, int currentUserId)
        {
            throw new System.NotImplementedException();
        }
    }
}