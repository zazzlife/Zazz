using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IPhotoVoteRepository
    {
        void InsertGraph(PhotoVote vote);

        bool Exists(int photoId, int userId);

        bool GetVoteCounts(int photoId);

        void Remove(PhotoVote vote);

        void Remove(int photoId, int userId);
    }
}