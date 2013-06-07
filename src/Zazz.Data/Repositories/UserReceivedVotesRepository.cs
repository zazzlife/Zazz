using System.Data.Entity;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class UserReceivedVotesRepository : IUserReceivedVotesRepository
    {
        private readonly ZazzDbContext _context;
        private readonly IDbSet<UserReceivedVotes> _dbSet;

        public UserReceivedVotesRepository(ZazzDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<UserReceivedVotes>();
        }

        public int GetCount(int userId)
        {
            throw new System.NotImplementedException();
        }

        public int Increment(int userId)
        {
            throw new System.NotImplementedException();
        }

        public int Decrement(int userId)
        {
            throw new System.NotImplementedException();
        }
    }
}