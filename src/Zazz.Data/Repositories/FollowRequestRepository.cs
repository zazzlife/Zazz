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
    public class FollowRequestRepository : IFollowRequestRepository
    {
        private readonly DbContext _dbContext;
        private readonly DbSet<FollowRequest> _dbSet;

        public FollowRequestRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<FollowRequest>();
        }

        public FollowRequest GetFollowRequest(int fromUserId, int toUserId)
        {
            return _dbSet
                .Where(f => f.FromUserId == fromUserId)
                .Where(f => f.ToUserId == toUserId)
                .SingleOrDefault();
        }

        public void InsertGraph(FollowRequest followRequest)
        {
            _dbSet.Add(followRequest);
        }

        public int GetReceivedRequestsCount(int userId)
        {
            return _dbSet.Count(r => r.ToUserId == userId);
        }

        public IQueryable<FollowRequest> GetReceivedRequests(int userId)
        {
            return _dbSet.Where(r => r.ToUserId == userId);
        }

        public IQueryable<FollowRequest> GetSentRequests(int userId)
        {
            return _dbSet.Where(r => r.FromUserId == userId);
        }

        public void Remove(FollowRequest followRequest)
        {
            _dbContext.Entry(followRequest).State = EntityState.Deleted;
        }

        public void Remove(int fromUserId, int toUserId)
        {
            var item = _dbSet
                .Where(r => r.FromUserId == fromUserId)
                .Where(r => r.ToUserId == toUserId)
                .SingleOrDefault();

            if (item == null)
                return;

            _dbContext.Entry(item).State = EntityState.Deleted;
        }

        public bool Exists(int fromUserId, int toUserId)
        {
            return _dbSet.Where(r => r.FromUserId == fromUserId)
                        .Where(r => r.ToUserId == toUserId)
                        .Any();
        }
    }
}