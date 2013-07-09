using System.Linq;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IUserPointRepository
    {
        IQueryable<UserPoint> GetAll(int? userId = null, int? clubId = null);

        void ChangeUserPoints(int userId, int clubId, int amountToChange);
    }
}