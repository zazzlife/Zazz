using Zazz.Core.Interfaces;

namespace Zazz.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        internal static ICacheSystem<string, int> UserIdCache = new CircularBufferCache<string, int>(200);
        internal static ICacheSystem<int, string> PhotoUrlCache = new CircularBufferCache<int, string>(200);
        internal static ICacheSystem<int, string> DisplayNameCache = new CircularBufferCache<int, string>(200);

        public void AddUserId(string username, int userId)
        {
            UserIdCache.Add(username, userId);
        }

        public int GetUserId(string username)
        {
            return UserIdCache.TryGet(username);
        }

        public void AddUserDiplayName(int userId, string displayName)
        {
            DisplayNameCache.Add(userId, displayName);
        }

        public string GetUserDisplayName(int userId)
        {
            return DisplayNameCache.TryGet(userId);
        }

        public void AddUserPhotoUrl(int userId, string photoUrl)
        {
            PhotoUrlCache.Add(userId, photoUrl);
        }

        public string GetUserPhotoUrl(int userId)
        {
            return PhotoUrlCache.TryGet(userId);
        }

        public void RemoveUserCache(string username, int userId)
        {
            UserIdCache.Remove(username);
            DisplayNameCache.Remove(userId);
            PhotoUrlCache.Remove(userId);
        }
    }
}