using Zazz.Core.Interfaces;

namespace Zazz.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        private readonly ICacheSystem<string, int> _userIdCache;
        private readonly ICacheSystem<int, string> _displayNameCache;
        private readonly ICacheSystem<int, string> _photoUrlCache;

        public CacheService(ICacheSystem<string, int> userIdCache,
            ICacheSystem<int, string> displayNameCache, ICacheSystem<int, string> photoUrlCache)
        {
            _userIdCache = userIdCache;
            _displayNameCache = displayNameCache;
            _photoUrlCache = photoUrlCache;
        }

        public void AddUserId(string username, int userId)
        {
            _userIdCache.Add(username, userId);
        }

        public int GetUserId(string username)
        {
            return _userIdCache.TryGet(username);
        }

        public void AddUserDiplayName(int userId, string displayName)
        {
            _displayNameCache.Add(userId, displayName);
        }

        public string GetUserDisplayName(int userId)
        {
            return _displayNameCache.TryGet(userId);
        }

        public void AddUserPhotoUrl(int userId, string photoUrl)
        {
            _photoUrlCache.Add(userId, photoUrl);
        }

        public string GetUserPhotoUrl(int userId)
        {
            return _photoUrlCache.TryGet(userId);
        }

        public void RemoveUserCache(string username, int userId)
        {
            _userIdCache.Remove(username);
            _displayNameCache.Remove(userId);
            _photoUrlCache.Remove(userId);
        }
    }
}