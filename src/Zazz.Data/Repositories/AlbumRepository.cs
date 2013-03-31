using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Data.Repositories
{
    public class AlbumRepository : BaseRepository<Album>, IAlbumRepository
    {
        public AlbumRepository(DbContext dbContext) : base(dbContext)
        {
        }

        protected override int GetItemId(Album item)
        {
            throw new InvalidOperationException("You should always provide the id for updating the album, if it's new then use insert graph.");
        }

        public Task<int> GetOwnerIdAsync(int albumId)
        {
            return Task.Run(() => DbSet.Where(a => a.Id == albumId)
                                             .Select(a => a.UserId)
                                             .SingleOrDefault());
        }

        public IEnumerable<int> GetAlbumPhotoIds(int albumId)
        {
            return DbSet.Where(a => a.Id == albumId)
                        .SelectMany(a => a.Photos)
                        .Select(p => p.Id);
        }

        public Album GetByFacebookId(string fbId)
        {
            return DbSet.SingleOrDefault(a => a.FacebookId.Equals(fbId, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}