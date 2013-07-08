using System;
using System.Data.Entity;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class UserPointRepository : IUserPointRepository
    {
        private readonly ZazzDbContext _dbContext;

        private readonly IDbSet<UserPoint> _dbSet;

        public UserPointRepository(DbContext context)
        {
            _dbContext = context as ZazzDbContext;
            if (_dbContext == null)
                throw new InvalidCastException("Passed DbContext should be of type ZazzDbContext");

            _dbSet = _dbContext.Set<UserPoint>();
        }

        public IQueryable<UserPoint> GetAll(int? userId = null, int? clubId = null)
        {
            var query = _dbSet.AsQueryable();

            if (userId.HasValue)
                query = query.Where(p => p.UserId == userId);

            if (clubId.HasValue)
                query = query.Where(p => p.ClubId == clubId);

            return query;
        }

        public void InsertGraph(UserPoint point)
        {
            _dbSet.Add(point);
        }
    }
}