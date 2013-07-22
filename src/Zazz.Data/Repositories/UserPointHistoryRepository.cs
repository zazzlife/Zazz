using System.Data.Entity;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class UserPointHistoryRepository : BaseLongRepository<UserPointHistory>, IUserPointHistoryRepository
    {
        public UserPointHistoryRepository(DbContext dbContext) : base(dbContext)
        {}
    }
}