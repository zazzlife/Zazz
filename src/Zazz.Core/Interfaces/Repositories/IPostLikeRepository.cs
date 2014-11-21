using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface IPostLikeRepository
    {
        void InsertGraph(PostLike postLike);

        bool Exists(int postId, int userId);

        int GetLikesCount(int postId);

        void Remove(PostLike postLike);

        void Remove(int postId, int userId);
    }
}
