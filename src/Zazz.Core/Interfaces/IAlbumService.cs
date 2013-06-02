using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.DTOs;

namespace Zazz.Core.Interfaces
{
    public interface IAlbumService
    {
        AlbumWithThumbnailDTO GetAlbumWithThumbnail(int albumId);

        Album GetAlbum(int albumId, bool includePhotos = false);

        IQueryable<Album> GetUserAlbums(int userId, bool includePhotos = false);

        void CreateAlbum(Album album);

        void UpdateAlbum(int albumId, string newName, int currentUserId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="albumId">Id of the album to delete</param>
        /// <param name="currentUserId">Id of the user that is updating the album. Required for security checks</param>
        /// <returns></returns>
        void DeleteAlbum(int albumId, int currentUserId);
    }
}