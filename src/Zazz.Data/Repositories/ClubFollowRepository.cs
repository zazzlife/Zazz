using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class ClubFollowRepository : BaseRepository<ClubFollow>, IClubFollowRepository
    {
        public ClubFollowRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(ClubFollow item)
        {
            if (item.ClubId == default (int) || item.UserId == default (int))
                throw new ArgumentException("Club Id or User Id cannot be 0");

            return DbSet.Where(f => f.ClubId == item.ClubId)
                        .Where(f => f.UserId == item.UserId)
                        .Select(f => f.Id)
                        .SingleOrDefault();
        }

        public Task<bool> ExistsAsync(int userId, int clubId)
        {
            return Task.Run(() => DbSet.Where(f => f.ClubId == clubId)
                                       .Where(f => f.UserId == userId)
                                       .Any());
        }

        public async Task RemoveAsync(int userId, int clubId)
        {
            var item = await Task.Run(() => DbSet.Where(f => f.ClubId == clubId)
                                           .Where(f => f.UserId == userId)
                                           .SingleOrDefault());
            if (item == null)
                return;

            DbContext.Entry(item).State = EntityState.Deleted;
        }
    }
}