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
    public class UserFollowRepository : BaseRepository<UserFollow>, IUserFollowRepository
    {
        public UserFollowRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(UserFollow item)
        {
            if (item.FromUserId == default (int) || item.ToUserId == default (int))
                throw new ArgumentException("FromUserId or ToUserId cannot be 0");

            return DbSet.Where(f => f.FromUserId == item.FromUserId)
                        .Where(f => f.ToUserId == item.ToUserId)
                        .Select(f => f.Id)
                        .SingleOrDefault();
        }

        public Task<IEnumerable<UserFollow>> GetUserFollowersAsync(int toUserId)
        {
            return Task.Run(() => DbSet.Where(f => f.ToUserId == toUserId).AsEnumerable());
        }

        public Task<IEnumerable<UserFollow>> GetUserFollowsAsync(int fromUserId)
        {
            return Task.Run(() => DbSet.Where(f => f.FromUserId == fromUserId).AsEnumerable());
        }

        public Task<bool> ExistsAsync(int fromUserId, int toUserId)
        {
            return Task.Run(() => DbSet.Where(f => f.FromUserId == fromUserId)
                                       .Where(f => f.ToUserId == toUserId)
                                       .Any());
        }

        public async Task DeleteAsync(int fromUserId, int toUserId)
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