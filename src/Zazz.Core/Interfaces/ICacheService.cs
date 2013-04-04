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

        void AddUserPhotoUrl(int userId, string photoUrl);

        string GetUserPhotoUrl(int userId);

        void RemoveUserCache(string username, int userId);
    }
}