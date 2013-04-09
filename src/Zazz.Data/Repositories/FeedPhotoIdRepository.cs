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

        public void RemoveByPhotoId(int photoId)
        {
            var items = DbSet.Where(p => p.PhotoId == photoId);
            foreach (var f in items)
                Remove(f);
        }

        public int GetCount(int feedId)
        {
            throw new System.NotImplementedException();
        }
    }
}