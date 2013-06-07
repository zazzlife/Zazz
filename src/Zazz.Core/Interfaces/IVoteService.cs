namespace Zazz.Core.Interfaces
{
    public interface IVoteService
    {
        int GetPhotoVoteCounts(int photoId);

        bool IsUserVotedOnPhoto(int photoId, int userId);

        void AddPhotoVote(int photoId, int currentUserId);

        void RemovePhotoVote(int photoId, int currentUserId);
    }
}