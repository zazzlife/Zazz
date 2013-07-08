using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class UserPointRepository : IUserPointRepository
    {
        public IQueryable<UserPoint> GetAll(int? userId = null, int? clubId = null)
        {
            throw new System.NotImplementedException();
        }

        public void InsertGraph(UserPoint point)
        {
            throw new System.NotImplementedException();
        }
    }
}