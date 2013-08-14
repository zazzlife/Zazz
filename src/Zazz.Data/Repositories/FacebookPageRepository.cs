using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Interfaces.Repositories;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class FacebookPageRepository : BaseRepository<FacebookPage>, IFacebookPageRepository
    {
        public FacebookPageRepository(DbContext dbContext) : base(dbContext)
        {}

        public IQueryable<FacebookPage> GetUserPages(int userId)
        {
            return DbSet.Where(f => f.UserId == userId);
        }

        public FacebookPage GetByFacebookPageId(string fbPageId)
        {
            return DbSet.Where(p => p.FacebookId.Equals(fbPageId))
                        .SingleOrDefault();
        }
    }
}