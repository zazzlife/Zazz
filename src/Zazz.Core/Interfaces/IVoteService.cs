namespace Zazz.Core.Interfaces
{
    public interface IVoteService
    {
        int GetPhotoVotesCount(int photoId);

        bool HasUserVotedOnPhoto(int photoId, int userId);

        void AddPhotoVote(int photoId, int currentUserId);

        void RemovePhotoVote(int photoId, int currentUserId);
    }
}