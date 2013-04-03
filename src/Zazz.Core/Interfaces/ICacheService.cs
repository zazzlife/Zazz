namespace Zazz.Core.Interfaces
{
    /// <summary>
    /// High level caching service. It should be a singleton
    /// </summary>
    public interface ICacheService
    {
        int GetUserId(string username);

        string GetUserDisplayName(string username);

        string GetUserPhotoUrl(string username);
    }
}