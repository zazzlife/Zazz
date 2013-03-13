using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IAlbumService : IDisposable
    {
        Task<Album> GetAlbumAsync(int albumId);

        string GenerateAlbumPath(int userId, int albumId);

        Task<List<Album>> GetUserAlbumsAsync(int userId, int skip, int take);

        Task<int> GetUserAlbumsCountAsync(int userId);

        Task CreateAlbumAsync(Album album);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="album">Album to update. Id should not be 0</param>
        /// <param name="currentUserId">Id of the user that is updating the album. Required for security checks</param>
        /// <returns></returns>
        Task UpdateAlbumAsync(Album album, int currentUserId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="albumId">Id of the album to delete</param>
        /// <param name="currentUserId">Id of the user that is updating the album. Required for security checks</param>
        /// <returns></returns>
        Task DeleteAlbumAsync(int albumId, int currentUserId);
    }
}