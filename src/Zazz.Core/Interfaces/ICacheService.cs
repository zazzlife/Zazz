namespace Zazz.Core.Interfaces
{
    /// <summary>
    /// High level caching service. It should be a singleton
    /// </summary>
    public interface ICacheService
    {
        void AddUserId(string username, int userId);

        int GetUserId(string username);

        void AddUserDiplayName(string username, string displayName);

        string GetUserDisplayName(string username);

        void AddUserPhotoUrl(string username, string photoUrl);

        string GetUserPhotoUrl(string username);
    }
}