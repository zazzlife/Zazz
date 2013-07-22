using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.DTOs;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface IAlbumRepository : IRepository<Album>
    {
        Album GetById(int id, bool includePhotos);

        IEnumerable<Album> GetLatestAlbums(int userId, int albumsCount = 3, int photosCount = 13);

        Album GetByFacebookId(string fbId);

        IEnumerable<int> GetPageAlbumIds(int pageId);
        
        AlbumWithThumbnailDTO GetAlbumWithThumbnail(int albumId);
        
        IQueryable<Album> GetUserAlbums(int userId, bool includePhotos = false);
    }
}