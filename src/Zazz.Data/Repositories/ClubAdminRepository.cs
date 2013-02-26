using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class ClubAdminRepository : BaseRepository<ClubAdmin>, IClubAdminRepository
    {
        public ClubAdminRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(ClubAdmin item)
        {
            if (item.ClubId == default (int) || item.UserId == default (int))
                throw new ArgumentException("Club id or User id cannot be 0");

            return DbSet.Where(a => a.ClubId == item.ClubId)
                        .Where(a => a.UserId == item.UserId)
                        .Select(a => a.Id)
                        .SingleOrDefault();
        }

        public Task<bool> ExistsAsync(int userId, int clubId)
        {
            return Task.Run(() => DbSet.Where(a => a.UserId == userId)
                                       .Where(a => a.ClubId == clubId)
                                       .Any());
        }
    }
}