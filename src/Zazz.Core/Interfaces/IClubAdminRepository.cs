using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IClubAdminRepository : IRepository<ClubAdmin>
    {
        Task<bool> ExistsAsync(int userId, int clubId);
    }
}