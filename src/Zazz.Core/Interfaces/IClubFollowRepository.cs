using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IClubFollowRepository : IRepository<ClubFollow>
    {
        Task<bool> ExistsAsync(int userId, int clubId);

        Task RemoveAsync(int userId, int clubId);
    }
}