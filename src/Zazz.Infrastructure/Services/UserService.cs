using System;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUoW _uoW;

        public UserService(IUoW uoW)
        {
            _uoW = uoW;
        }

        public AccountType GetUserAccountType(int userId)
        {
            return _uoW.UserRepository.GetUserAccountType(userId);
        }

        public int GetUserId(string username)
        {
            return _uoW.UserRepository.GetIdByUsername(username);
        }

        public Task<User> GetUserAsync(string username)
        {
            return _uoW.UserRepository.GetByUsernameAsync(username);
        }

        public string GetUserFullName(int userId)
        {
            return _uoW.UserRepository.GetUserFullName(userId);
        }

        public string GetUserFullName(string username)
        {
            return _uoW.UserRepository.GetUserFullName(username);
        }

        public string GetUserDisplayName(int userId)
        {
            var fullName = _uoW.UserRepository.GetUserFullName(userId);
            if (!String.IsNullOrEmpty(fullName))
            {
                return fullName;
            }
            else
            {
                return _uoW.UserRepository.GetUserName(userId);
            }
        }

        public string GetUserDisplayName(string username)
        {
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            _uoW.Dispose();
        }
    }
}