using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IClubPostCommentRepository : IRepository<ClubPostComment>
    {
        Task<IQueryable<ClubPostComment>> GetPostCommentsAsync(int postId);
    }
}