using System;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUoW _uoW;
        private readonly ICacheService _cacheService;

        public UserService(IUoW uoW, ICacheService cacheService)
        {
            _uoW = uoW;
            _cacheService = cacheService;
        }

        public AccountType GetUserAccountType(int userId)
        {
            return _uoW.UserRepository.GetUserAccountType(userId);
        }

        public int GetUserId(string username)
        {
            var cache = _cacheService.GetUserId(username);
            if (cache != default (int))
                return cache;

            var userId = _uoW.UserRepository.GetIdByUsername(username);
            _cacheService.AddUserId(username, userId);

            return userId;
        }

        public Task<User> GetUserAsync(string username)
        {
            return _uoW.UserRepository.GetByUsernameAsync(username);
        }

        public string GetUserDisplayName(int userId)
        {
            var cache = _cacheService.GetUserDisplayName(userId);
            if (!String.IsNullOrEmpty(cache))
                return cache;

            var fullName = _uoW.UserRepository.GetUserFullName(userId);
            if (!String.IsNullOrEmpty(fullName))
            {
                _cacheService.AddUserDiplayName(userId, fullName);
                return fullName;
            }
            else
            {
                var username = _uoW.UserRepository.GetUserName(userId);
                _cacheService.AddUserDiplayName(userId, username);
                return username;
            }
        }

        public string GetUserDisplayName(string username)
        {
            var userId = GetUserId(username);
            return GetUserDisplayName(userId);
        }

        public void Dispose()
        {
            _uoW.Dispose();
        }
    }
}