namespace Zazz.Core.Interfaces
{
    public interface IVoteService
    {
        void AddPhotoVote(int photoId, int currentUserId);

        void RemovePhotoVote(int photoId, int currentUserId);
    }
}