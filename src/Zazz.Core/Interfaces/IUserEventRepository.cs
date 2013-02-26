using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IUserEventRepository : IRepository<UserEvent>
    {
        Task<int> GetOwnerIdAsync(int eventId);
    }
}