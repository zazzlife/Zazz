using System;
using System.Data.Entity;
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
    }
}