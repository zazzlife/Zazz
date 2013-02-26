using System;
using System.Data.Entity;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class ClubImageRepository : BaseRepository<ClubImage>, IClubImageRepository
    {
        public ClubImageRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(ClubImage item)
        {
            throw new InvalidOperationException("You should always provide the id for updating the image, if it's new then use insert graph.");
        }
    }
}