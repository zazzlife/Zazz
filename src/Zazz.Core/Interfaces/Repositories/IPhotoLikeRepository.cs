using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface IPhotoLikeRepository
    {
        IQueryable<PhotoLike> GetUserReceivedLikes(int userId);

        void InsertGraph(PhotoLike photoLike);

        bool Exists(int photoId, int userId);

        int GetLikesCount(int photoId);

        void Remove(PhotoLike photoLike);

        void Remove(int photoId, int userId);
    }
}