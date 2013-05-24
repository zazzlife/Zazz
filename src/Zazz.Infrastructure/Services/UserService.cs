using System;
using System.Text;
using System.Threading.Tasks;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUoW _uoW;
        private readonly ICacheService _cacheService;
        private readonly ICryptoService _cryptoService;

        public UserService(IUoW uoW, ICacheService cacheService, ICryptoService cryptoService)
        {
            _uoW = uoW;
            _cacheService = cacheService;
            _cryptoService = cryptoService;
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

        public User GetUser(string username, bool includeDetails = false, bool includeClubDetails = false,
                     bool includeWeeklies = false, bool includePreferences = false)
        {
            return _uoW.UserRepository
                .GetByUsername(username, includeDetails, includeClubDetails, includeWeeklies, includePreferences);
        }

        public User GetUser(int userId, bool includeDetails = false, bool includeClubDetails = false,
            bool includeWeeklies = false, bool includePreferences = false)
        {
            return _uoW.UserRepository
                .GetById(userId, includeDetails, includeClubDetails, includeWeeklies, includePreferences);
        }

        public byte[] GetUserPassword(int userId)
        {
            var cache = _cacheService.GetUserPassword(userId);
            if (cache != default (byte[]))
                return cache;

            var user = _uoW.UserRepository.GetById(userId);
            if (user == null)
                throw new NotFoundException();

            var password = _cryptoService.DecryptPassword(user.Password, user.PasswordIV);
            var passwordBuffer = Encoding.UTF8.GetBytes(password);

            _cacheService.AddUserPassword(userId, passwordBuffer);

            return passwordBuffer;
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
    }
}