using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IUserEventRepository : IRepository<UserEvent>
    {
        Task<bool> IsAuthorizedToUpdate(int eventId, int userId);
    }
}