using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class FollowRepository : BaseRepository<Follow>, IFollowRepository
    {
        public FollowRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(Follow item)
        {
            if (item.FromUserId == default (int) || item.ToUserId == default (int))
                throw new ArgumentException("FromUserId or ToUserId cannot be 0");

            return DbSet.Where(f => f.FromUserId == item.FromUserId)
                        .Where(f => f.ToUserId == item.ToUserId)
                        .Select(f => f.Id)
                        .SingleOrDefault();
        }

        public IQueryable<Follow> GetUserFollowers(int toUserId)
        {
            return DbSet.Where(f => f.ToUserId == toUserId);
        }

        public IEnumerable<Follow> GetUserFollows(int fromUserId)
        {
            return DbSet.Where(f => f.FromUserId == fromUserId).AsEnumerable();
        }

        public IEnumerable<int> GetFollowsUserIds(int fromUserId)
        {
            return DbSet.Where(f => f.FromUserId == fromUserId)
                        .Select(f => f.ToUserId);
        }

        public int GetFollowersCount(int userId)
        {
            return DbSet.Count(f => f.ToUserId == userId);
        }

        public bool Exists(int fromUserId, int toUserId)
        {
            return DbSet.Where(f => f.FromUserId == fromUserId)
                        .Where(f => f.ToUserId == toUserId)
                        .Any();
        }

        public void Remove(int fromUserId, int toUserId)
        {
            var item = DbSet.Where(f => f.FromUserId == fromUserId)
                            .Where(f => f.ToUserId == toUserId)
                            .SingleOrDefault();

            if (item == null)
                return;

            DbContext.Entry(item).State = EntityState.Deleted;
        }
    }
}