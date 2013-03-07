using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IUserEventCommentRepository : IRepository<Comment>
    {
        Task<IQueryable<Comment>> GetEventCommentsAsync(int eventId);
    }
}