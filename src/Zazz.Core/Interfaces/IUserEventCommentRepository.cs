using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IUserEventCommentRepository : IRepository<UserEventComment>
    {
        Task<IQueryable<UserEventComment>> GetEventCommentsAsync(int eventId);
    }
}