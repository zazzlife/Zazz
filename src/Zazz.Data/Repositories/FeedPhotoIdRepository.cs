using System;
using System.Data.Entity;
using System.Linq;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class FeedPhotoIdRepository : BaseRepository<FeedPhotoId>, IFeedPhotoIdRepository
    {
        public FeedPhotoIdRepository(DbContext dbContext) : base(dbContext)
        {}

        protected override int GetItemId(FeedPhotoId item)
        {
            throw new InvalidOperationException("You should always provide the id for updating the record, if it's new then use insert graph.");
        }

        public int RemoveByPhotoIdAndReturnFeedId(int photoId)
        {
            var item = DbSet.SingleOrDefault(p => p.PhotoId == photoId);
            if (item != null)
            {
                Remove(item);
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