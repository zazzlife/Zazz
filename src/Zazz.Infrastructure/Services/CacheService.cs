using Zazz.Core.Interfaces;

namespace Zazz.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        private readonly ICacheSystem<string, int> _userIdCache;
        private readonly ICacheSystem<string, string> _displayNameCache;
        private readonly ICacheSystem<string, string> _photoUrlCache;

        public CacheService(ICacheSystem<string, int> userIdCache,
            ICacheSystem<string, string> displayNameCache, ICacheSystem<string, string> photoUrlCache)
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

        public void AddUserDiplayName(string username, string displayName)
        {
            _displayNameCache.Add(username, displayName);
        }

        public string GetUserDisplayName(string username)
        {
            return _displayNameCache.TryGet(username);
        }

        public void AddUserPhotoUrl(string username, string photoUrl)
        {
            _photoUrlCache.Add(username, photoUrl);
        }

        public string GetUserPhotoUrl(string username)
        {
            return _photoUrlCache.TryGet(username);
        }
    }
}