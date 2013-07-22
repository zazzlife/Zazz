using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class FollowRepository : IFollowRepository
    {
        private readonly DbContext _dbContext;
        private readonly DbSet<Follow> _dbSet;

        public FollowRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<Follow>();
        }

        public void InsertGraph(Follow follow)
        {
            _dbSet.Add(follow);
        }

        public IQueryable<Follow> GetUserFollowers(int toUserId)
        {
            return _dbSet.Where(f => f.ToUserId == toUserId);
        }

        public IQueryable<Follow> GetUserFollows(int fromUserId)
        {
            return _dbSet.Where(f => f.FromUserId == fromUserId);
        }

        public IEnumerable<int> GetFollowsUserIds(int fromUserId)
        {
            return _dbSet.Where(f => f.FromUserId == fromUserId)
                        .Select(f => f.ToUserId);
        }

        public int GetFollowersCount(int userId)
        {
            return _dbSet.Count(f => f.ToUserId == userId);
        }

        public bool Exists(int fromUserId, int toUserId)
        {
            return _dbSet.Where(f => f.FromUserId == fromUserId)
                        .Where(f => f.ToUserId == toUserId)
                        .Any();
        }

        public void Remove(int fromUserId, int toUserId)
        {
            var item = _dbSet.Where(f => f.FromUserId == fromUserId)
                            .Where(f => f.ToUserId == toUserId)
                            .SingleOrDefault();

            if (item == null)
                return;

            _dbContext.Entry(item).State = EntityState.Deleted;
        }
    }
}