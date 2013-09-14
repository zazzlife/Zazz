using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface IPhotoVoteRepository
    {
        void InsertGraph(PhotoVote vote);

        bool Exists(int photoId, int userId);

        int GetPhotoVotesCount(int photoId);

        void Remove(PhotoVote vote);

        void Remove(int photoId, int userId);
    }
}