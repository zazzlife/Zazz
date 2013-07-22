using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface IUserPointRepository
    {
        IQueryable<UserPoint> GetAll(int? userId = null, int? clubId = null,
            bool includeUser = false, bool includeClub = false);

        void ChangeUserPoints(int userId, int clubId, int amountToChange);
    }
}