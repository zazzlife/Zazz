using Zazz.Core.Interfaces;
using Zazz.Core.Models;

namespace Zazz.Infrastructure.Services
{
    // this class is registered as a singleton, if later on you added a dependency remove the singleton flag.
    public class CacheService : ICacheService
    {
        //These properties should be internal so they are accessable by unit test project.
        internal static ICacheSystem<string, int> UserIdCache = new CircularBufferCache<string, int>(500);
        internal static ICacheSystem<int, PhotoLinks> PhotoUrlCache = new CircularBufferCache<int, PhotoLinks>(200);
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

        public void AddUserPhotoUrl(int userId, PhotoLinks photoUrl)
        {
            PhotoUrlCache.Add(userId, photoUrl);
        }

        public PhotoLinks GetUserPhotoUrl(int userId)
        {
            return PhotoUrlCache.TryGet(userId);
        }

        public void RemoveUserDisplayName(int userId)
        {
            DisplayNameCache.Remove(userId);
        }

        public void RemoveUserPhotoUrl(int userId)
        {
            PhotoUrlCache.Remove(userId);
        }
    }
}