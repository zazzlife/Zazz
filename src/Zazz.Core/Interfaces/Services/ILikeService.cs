namespace Zazz.Core.Interfaces.Services
{
    public interface ILikeService
    {
        int GetPhotoLikesCount(int photoId);

        bool PhotoLikeExists(int photoId, int userId);

        void AddPhotoLike(int photoId, int currentUserId);

        void RemovePhotoLike(int photoId, int currentUserId);
    }
}