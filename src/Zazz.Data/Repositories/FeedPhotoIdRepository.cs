using System;
using System.Data.Entity;
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
            throw new System.NotImplementedException();
        }

        public int GetCount(int feedId)
        {
            throw new System.NotImplementedException();
        }
    }
}