using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IAlbumService
    {
        Album GetAlbum(int albumId, bool includePhotos = false);

        List<Album> GetUserAlbums(int userId, int skip, int take);

        List<Album> GetUserAlbums(int userId);

        int GetUserAlbumsCount(int userId);

        void CreateAlbum(Album album);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="album">Album to update. Id should not be 0</param>
        /// <param name="currentUserId">Id of the user that is updating the album. Required for security checks</param>
        /// <returns></returns>
        void UpdateAlbum(Album album, int currentUserId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="albumId">Id of the album to delete</param>
        /// <param name="currentUserId">Id of the user that is updating the album. Required for security checks</param>
        /// <returns></returns>
        void DeleteAlbum(int albumId, int currentUserId);
    }
}