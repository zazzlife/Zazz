using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IPhotoService : IDisposable
    {
        List<Photo> GetAlbumPhotos(int albumId, int skip, int take);

        int GetAlbumPhotosCount(int albumId);

        string GeneratePhotoUrl(int userId, int albumId, int photoId);

        string GeneratePhotoFilePath(int userId, int albumId, int photoId);

        Task<string> GetPhotoDescriptionAsync(int photoId);
        
        Task<int> SavePhotoAsync(Photo photo, Stream data);

        Task RemovePhotoAsync(int photoId, int currentUserId);

        Task UpdatePhotoAsync(Photo photo, int currentUserId);
    }
}