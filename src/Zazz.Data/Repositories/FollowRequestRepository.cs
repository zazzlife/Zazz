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
    public class FollowRequestRepository : BaseRepository<FollowRequest>, IFollowRequestRepository
    {
        public FollowRequestRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(FollowRequest item)
        {
            if (item.FromUserId == default (int) || item.ToUserId == default (int))
                throw new ArgumentException("From user id and to user id must be supplied");

            return DbSet.Where(r => r.FromUserId == item.FromUserId)
                        .Where(r => r.ToUserId == item.ToUserId)
                        .Select(r => r.Id)
                        .SingleOrDefault();
        }

        public int GetReceivedRequestsCount(int userId)
        {
            return DbSet.Count(r => r.ToUserId == userId);
        }

        public List<FollowRequest> GetReceivedRequests(int userId)
        {
            return DbSet.Where(r => r.ToUserId == userId).ToList();
        }

        public List<FollowRequest> GetSentRequests(int userId)
        {
            return DbSet.Where(r => r.FromUserId == userId).ToList();
        }

        public void Remove(int fromUserId, int toUserId)
        {
            var item = DbSet
                .Where(r => r.FromUserId == fromUserId)
                .Where(r => r.ToUserId == toUserId)
                .SingleOrDefault();

            if (item == null)
                return;

            DbContext.Entry(item).State = EntityState.Deleted;
        }

        public bool Exists(int fromUserId, int toUserId)
        {
            return DbSet.Where(r => r.FromUserId == fromUserId)
                        .Where(r => r.ToUserId == toUserId)
                        .Any();
        }
    }
}