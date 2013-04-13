using Zazz.Core.Models;

namespace Zazz.Core.Interfaces
{
    /// <summary>
    /// High level caching service. It should be a singleton
    /// </summary>
    public interface ICacheService
    {
        void AddUserId(string username, int userId);

        int GetUserId(string username);

        void AddUserDiplayName(int userId, string displayName);

        string GetUserDisplayName(int userId);

        void AddUserPhotoUrl(int userId, PhotoLinks photoUrl);

        PhotoLinks GetUserPhotoUrl(int userId);

        void RemoveUserDisplayName(int userId);

        void RemoveUserPhotoUrl(int userId);
    }
}