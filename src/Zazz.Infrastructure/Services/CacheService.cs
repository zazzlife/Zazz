using Zazz.Core.Interfaces;

namespace Zazz.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        internal static ICacheSystem<string, int> _userIdCache = new CircularBufferCache<string, int>(200);
        internal static ICacheSystem<int, string> _displayNameCache = new CircularBufferCache<int, string>(200);
        internal static ICacheSystem<int, string> _photoUrlCache = new CircularBufferCache<int, string>(200);

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