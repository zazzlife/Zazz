using System;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class UserPointRepository : IUserPointRepository
    {
        private readonly ZazzDbContext _context;

        private readonly IDbSet<UserPoint> _dbSet;

        public UserPointRepository(DbContext context)
        {
            _context = context as ZazzDbContext;
            if (_context == null)
                throw new InvalidCastException("Passed DbContext should be of type ZazzDbContext");

            _dbSet = _context.Set<UserPoint>();
        }

        public IQueryable<UserPoint> GetAll(int? userId = null, int? clubId = null,
            bool includeUser = false, bool includeClub = false)
        {
            var query = _dbSet.AsQueryable();

            if (includeUser)
                query = query.Include(p => p.User);

            if (includeClub)
                query = query.Include(p => p.Club);

            if (userId.HasValue)
                query = query.Where(p => p.UserId == userId);

            if (clubId.HasValue)
                query = query.Where(p => p.ClubId == clubId);

            return query;
        }

        public void ChangeUserPoints(int userId, int clubId, int amountToChange)
        {
            var newLine = Environment.NewLine;
            var query =
                "set nocount on" + newLine +
                "IF EXISTS(SELECT 1 FROM dbo.UserPoints WHERE UserId = @userId AND ClubId = @clubId)" + newLine +
                    "BEGIN" + newLine +
                        "UPDATE dbo.UserPoints SET Points = Points + @amount WHERE UserId = @userId AND ClubId = @clubId" + newLine +
                    "END" + newLine +
                "ELSE" + newLine +
                    "BEGIN" + newLine +
                        "INSERT INTO dbo.UserPoints (UserId, ClubId, Points) VALUES (@userId, @clubId , @amount)" + newLine +
                    "END";

            var userIdParam = new SqlParameter("userId", SqlDbType.Int)
                              {
                                  Value = userId
                              };

            var clubIdParam = new SqlParameter("clubId", SqlDbType.Int)
                              {
                                  Value = clubId
                              };

            var amountParam = new SqlParameter("amount", SqlDbType.Int)
                              {
                                  Value = amountToChange
                              };

            _context.Database.ExecuteSqlCommand(query, userIdParam, clubIdParam, amountParam);
        }
    }
}