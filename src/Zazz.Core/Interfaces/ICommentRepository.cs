using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface ICommentRepository : IRepository<Comment>
    {
        Task<IQueryable<Comment>> GetCommentsAsync(int postId);
    }
}