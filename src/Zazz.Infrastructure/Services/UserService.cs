using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Services;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

namespace Zazz.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUoW _uoW;
        private readonly ICacheService _cacheService;
        private readonly ICryptoService _cryptoService;
        private readonly IPhotoService _photoService;

        public UserService(IUoW uoW, ICacheService cacheService, ICryptoService cryptoService,
            IPhotoService photoService)
        {
            _uoW = uoW;
            _cacheService = cacheService;
            _cryptoService = cryptoService;
            _photoService = photoService;
        }

        public AccountType GetAccountType(int userId)
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
            var user = _uoW.UserRepository
                .GetByUsername(username, includeDetails, includeClubDetails, includeWeeklies, includePreferences);

            if (user == null)
                throw new NotFoundException();

            return user;
        }

        public User GetUser(int userId, bool includeDetails = false, bool includeClubDetails = false,
            bool includeWeeklies = false, bool includePreferences = false)
        {
            var user = _uoW.UserRepository
                .GetById(userId, includeDetails, includeClubDetails, includeWeeklies, includePreferences);

            if (user == null)
                throw new NotFoundException();

            return user;
        }

        public IEnumerable<UserSearchResult> Search(string name)
        {
            var users = _uoW.UserRepository.GetAll()
                            .Where(u =>
                                   u.UserDetail.FullName.Contains(name) ||
                                   u.Username.Contains(name) ||
                                   u.ClubDetail.ClubName.Contains(name))
                            .Select(u => new
                                         {
                                             u.Id,
                                             u.AccountType,
                                             u.Username,
                                             u.UserDetail.FullName,
                                             u.ClubDetail.ClubName
                                         })
                            .Take(5)
                            .ToList();


            return users.Select(u => new UserSearchResult
                                     {
                                         UserId = u.Id,
                                         AccountType = u.AccountType,
                                         DisplayName = (u.AccountType == AccountType.Club &&
                                                        !String.IsNullOrEmpty(u.ClubName))
                                                           ? u.ClubName
                                                           : (u.AccountType == AccountType.User &&
                                                              !String.IsNullOrEmpty(u.FullName))
                                                                 ? u.FullName
                                                                 : u.Username,
                                         DisplayPhoto = _photoService.GetUserDisplayPhoto(u.Id)
                                     });
        }

        public string GetUserDisplayName(int userId)
        {
            var cache = _cacheService.GetUserDisplayName(userId);
            if (!String.IsNullOrEmpty(cache))
                return cache;

            var displayName = _uoW.UserRepository.GetDisplayName(userId);
            if (displayName == null)
                throw new NotFoundException();

            _cacheService.AddUserDiplayName(userId, displayName);
            return displayName;
        }

        public string GetUserDisplayName(string username)
        {
            var userId = GetUserId(username);
            return GetUserDisplayName(userId);
        }

        public string GetAccessToken(int userId, OAuthProvider provider)
        {
            return _uoW.LinkedAccountRepository.GetAccessToken(userId, provider);
        }

        public bool OAuthAccountExists(int userId, OAuthProvider provider)
        {
            return _uoW.LinkedAccountRepository.Exists(userId, provider);
        }


        public void ChangeProfilePic(int userId, int? photoId)
        {
            var user = _uoW.UserRepository.GetById(userId);
            if (user == null)
                throw new NotFoundException("user not found");

            if (!photoId.HasValue)
            {
                user.ProfilePhotoId = null;
            }
            else
            {
                var photo = _uoW.PhotoRepository.GetById(photoId.Value);
                if (photo == null)
                    throw new NotFoundException("photo not found");

                if (user.Id != photo.UserId)
                    throw new SecurityException();

                user.ProfilePhotoId = photoId;
            }

            _cacheService.RemoveUserPhotoUrl(userId);
            _uoW.SaveChanges();
        }

        public void ChangeCoverPic(int userId, int? photoId)
        {
            var user = _uoW.UserRepository.GetById(userId, false, true);
            if (user == null)
                throw new NotFoundException("user not found");

            if (user.AccountType != AccountType.Club)
                throw new InvalidOperationException("user was not club");

            if (!photoId.HasValue)
            {
                user.ClubDetail.CoverPhotoId = photoId;
            }
            else
            {
                var photo = _uoW.PhotoRepository.GetById(photoId.Value);
                if (photo == null)
                    throw new NotFoundException("photo not found");
            }

            _uoW.SaveChanges();
        }
    }
}