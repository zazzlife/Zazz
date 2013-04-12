using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface ICommentRepository : IRepository<Comment>
    {
        IQueryable<Comment> GetComments(int eventId);

        void RemovePhotoComments(int photoId);

        void RemoveEventComments(int eventId);

        void RemovePostComments(int postId);
    }
}