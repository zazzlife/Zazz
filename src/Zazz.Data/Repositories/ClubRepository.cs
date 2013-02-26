using System;
using System.Data.Entity;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class ClubRepository : BaseRepository<Club>, IClubRepository
    {
        public ClubRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(Club item)
        {
            throw new InvalidOperationException("You should always provide the id for updating the club, if it's new then use insert graph.");
        }
    }
}