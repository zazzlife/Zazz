using System;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
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
            return _dbSet.Where(u => u.UserId == userId)
                         .Select(u => u.Count)
                         .SingleOrDefault();
        }

        public void Increment(int userId)
        {
            var newLine = Environment.NewLine;
            var query =
                "set nocount on" + newLine +
                "IF EXISTS(SELECT 1 FROM dbo.UserReceivedVotes WHERE UserId = @userId)" + newLine +
                    "BEGIN" + newLine +
                        "UPDATE dbo.UserReceivedVotes SET Count = Count + 1, LastUpdate = GETUTCDATE() WHERE UserId = @userId" + newLine +
                    "END" + newLine +
                "ELSE" + newLine +
                    "BEGIN" + newLine +
                        "INSERT INTO dbo.UserReceivedVotes (UserId, Count, LastUpdate) VALUES (@userId, 1, GETUTCDATE())" + newLine +
                    "END";

            var userIdParam = new SqlParameter("userId", SqlDbType.Int)
                              {
                                  Value = userId
                              };

            _context.Database.ExecuteSqlCommand(query, userIdParam);
        }

        public void Decrement(int userId)
        {
            var newLine = Environment.NewLine;
            var query =
                "set nocount on" + newLine +
                "IF EXISTS(SELECT 1 FROM dbo.UserReceivedVotes WHERE UserId = @userId)" + newLine +
                    "BEGIN" + newLine +
                        "UPDATE dbo.UserReceivedVotes SET Count = Count - 1, LastUpdate = GETUTCDATE() WHERE UserId = @userId" + newLine +
                    "END" + newLine +
                "ELSE" + newLine +
                    "BEGIN" + newLine +
                        "INSERT INTO dbo.UserReceivedVotes (UserId, Count, LastUpdate) VALUES (@userId, 0, GETUTCDATE())" + newLine +
                    "END";

            var userIdParam = new SqlParameter("userId", SqlDbType.Int)
                              {
                                  Value = userId
                              };

            _context.Database.ExecuteSqlCommand(query, userIdParam);
        }
    }
}