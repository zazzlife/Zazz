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

        public Task<IEnumerable<Follow>> GetUserFollowersAsync(int toUserId)
        {
            return Task.Run(() => DbSet.Where(f => f.ToUserId == toUserId).AsEnumerable());
        }

        public Task<IEnumerable<Follow>> GetUserFollowsAsync(int fromUserId)
        {
            return Task.Run(() => DbSet.Where(f => f.FromUserId == fromUserId).AsEnumerable());
        }

        public int GetFollowersCount(int userId)
        {
            return DbSet.Count(f => f.ToUserId == userId);
        }

        public Task<bool> ExistsAsync(int fromUserId, int toUserId)
        {
            return Task.Run(() => DbSet.Where(f => f.FromUserId == fromUserId)
                                       .Where(f => f.ToUserId == toUserId)
                                       .Any());
        }

        public async Task RemoveAsync(int fromUserId, int toUserId)
        {
            var item = await Task.Run(() => DbSet.Where(f => f.FromUserId == fromUserId)
                                                 .Where(f => f.ToUserId == toUserId)
                                                 .SingleOrDefault());

            if (item == null)
                return;

            DbContext.Entry(item).State = EntityState.Deleted;
        }
    }
}