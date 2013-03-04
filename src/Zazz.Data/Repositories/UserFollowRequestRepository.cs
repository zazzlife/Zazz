using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class UserFollowRequestRepository : BaseRepository<UserFollowRequest>, IUserFollowRequestRepository
    {
        public UserFollowRequestRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(UserFollowRequest item)
        {
            if (item.FromUserId == default (int) || item.ToUserId == default (int))
                throw new ArgumentException("From user id and to user id must be supplied");

            return DbSet.Where(r => r.FromUserId == item.FromUserId)
                        .Where(r => r.ToUserId == item.ToUserId)
                        .Select(r => r.Id)
                        .SingleOrDefault();
        }

        public Task<int> GetReceivedRequestsCountAsync(int userId)
        {
            return Task.Run(() => DbSet.Count(r => r.ToUserId == userId));
        }

        public Task<List<UserFollowRequest>> GetReceivedRequestsAsync(int userId)
        {
            return Task.Run(() => DbSet.Where(r => r.ToUserId == userId).ToList());
        }

        public Task<List<UserFollowRequest>> GetSentRequestsAsync(int userId)
        {
            return Task.Run(() => DbSet.Where(r => r.FromUserId == userId).ToList());
        }

        public async Task RemoveAsync(int fromUserId, int toUserId)
        {
            var item = await Task.Run(() => DbSet
                                                .Where(r => r.FromUserId == fromUserId)
                                                .Where(r => r.ToUserId == toUserId)
                                                .SingleOrDefault());

            if (item == null)
                return;

            DbContext.Entry(item).State = EntityState.Deleted;
        }

        public Task<bool> ExistsAsync(int fromUserId, int toUserId)
        {
            return Task.Run(() => DbSet.Where(r => r.FromUserId == fromUserId)
                                       .Where(r => r.ToUserId == toUserId)
                                       .Any());
        }
    }
}