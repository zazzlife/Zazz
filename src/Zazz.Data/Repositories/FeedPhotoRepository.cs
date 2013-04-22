using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class FeedPhotoRepository : IFeedPhotoRepository
    {
        private readonly ZazzDbContext _dbContext;
        private IDbSet<FeedPhoto> DbSet { get; set; }

        public FeedPhotoRepository(DbContext dbContext)
        {
            var zazzContext = dbContext as ZazzDbContext;

            if (zazzContext == null)
                throw new ArgumentException("DbContext must be of type ZazzDbContext", "dbContext");

            _dbContext = zazzContext;
            DbSet = dbContext.Set<FeedPhoto>();
        }

        public int RemoveByPhotoIdAndReturnFeedId(int photoId)
        {
            var item = DbSet.SingleOrDefault(p => p.PhotoId == photoId);
            if (item != null)
            {
                _dbContext.Entry(item).State = EntityState.Deleted;
                return item.FeedId;
            }

            return 0;
        }

        public int GetCount(int feedId)
        {
            return DbSet.Count(p => p.FeedId == feedId);
        }
    }
}