using Zazz.Core.Interfaces;

namespace Zazz.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUoW _uoW;

        public UserService(IUoW uoW)
        {
            _uoW = uoW;
        }

        public int GetUserId(string username)
        {
            return _uoW.UserRepository.GetIdByUsername(username);
        }

        public void Dispose()
        {
            _uoW.Dispose();
        }
    }
}