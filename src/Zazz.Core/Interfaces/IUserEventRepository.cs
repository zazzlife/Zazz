using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IUserEventRepository : IRepository<Post>
    {
        Task<int> GetOwnerIdAsync(int eventId);
    }
}