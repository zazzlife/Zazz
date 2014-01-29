using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;

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

        public IQueryable<User> GetClubsThatUserFollows(int userId)
        {
            return _dbSet
                .Where(f => f.FromUserId == userId)
                .Where(f => f.ToUser.AccountType == AccountType.Club)
                .Select(f => f.ToUser)
                .Include(f => f.ClubDetail);
        }

        public IQueryable<User> GetClubsThatUserDoesNotFollow(int userId)
        {
            //getting all club ids
            var context = _dbContext as ZazzDbContext;

            var allClubIds = context.Users.Where(u => u.AccountType == AccountType.Club)
                .Select(u => u.Id);

            var allFollows = _dbSet
                .Where(f => f.FromUserId == userId)
                .Select(f => f.ToUserId);

            var remainingClubs = allClubIds.Where(id => !allFollows.Contains(id));

            return context.Users
                .Where(u => remainingClubs.Contains(u.Id))
                .Include(u => u.ClubDetail);
        }
    }
}