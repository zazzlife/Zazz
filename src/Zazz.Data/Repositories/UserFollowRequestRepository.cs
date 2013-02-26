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
            if (item.UserId == default (int) || item.FollowId == default (int))
                throw new ArgumentException("From user id and to user id must be supplied");

            return DbSet.Where(r => r.UserId == item.UserId)
                        .Where(r => r.FollowId == item.FollowId)
                        .Select(r => r.Id)
                        .SingleOrDefault();
        }

        public Task<List<UserFollowRequest>> GetReceivedRequestsAsync(int userId)
        {
            return Task.Run(() => DbSet.Where(r => r.FollowId == userId).ToList());
        }

        public Task<List<UserFollowRequest>> GetSentRequestsAsync(int userId)
        {
            return Task.Run(() => DbSet.Where(r => r.UserId == userId).ToList());
        }

        public async Task RemoveAsync(int fromUserId, int toUserId)
        {
            var item = await Task.Run(() => DbSet
                                                .Where(r => r.UserId == fromUserId)
                                                .Where(r => r.FollowId == toUserId)
                                                .SingleOrDefault());

            if (item == null)
                return;

            DbContext.Entry(item).State = EntityState.Deleted;
        }
    }
}