namespace Zazz.Core.Interfaces.Services
{
    public interface ILikeService
    {
        int GetPhotoLikesCount(int photoId);

        bool PhotoLikeExists(int photoId, int userId);

        void AddPhotoLike(int photoId, int currentUserId);

        void RemovePhotoLike(int photoId, int currentUserId);

        int GetPostLikesCount(int postId);

        bool PostLikeExists(int postId, int userId);

        void AddPostLike(int postId, int currentUserId);

        void RemovePostLike(int postId, int currentUserId);
    }
}