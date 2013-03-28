using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class FacebookPageRepository : BaseRepository<FacebookPage>, IFacebookPageRepository
    {
        public FacebookPageRepository(DbContext dbContext) : base(dbContext)
        {}

        protected override int GetItemId(FacebookPage item)
        {
            throw new InvalidOperationException("You must provide the Id for updating. Use insert graph for insert");
        }

        public List<string> GetUserPageFacebookIds(int userId)
        {
            return DbSet.Where(p => p.UserId == userId)
                        .Select(p => p.FacebookId)
                        .ToList();
        }
    }
}