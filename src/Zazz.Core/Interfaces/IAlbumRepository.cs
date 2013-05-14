using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IAlbumRepository : IRepository<Album>
    {
        IEnumerable<Album> GetLatestAlbums(int userId, int albumsCount = 3, int photosCount = 13);

        int GetOwnerId(int albumId);

        IEnumerable<int> GetAlbumPhotoIds(int albumId);

        Album GetByFacebookId(string fbId);

        IEnumerable<int> GetPageAlbumIds(int pageId);
    }
}