using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IPhotoService : IDisposable
    {
        Task<Photo> GetPhotoAsync(int id);

        string GeneratePhotoUrl(int userId, int albumId, int photoId);

        string GeneratePhotoFilePath(int userId, int albumId, int photoId);

        Task<string> GetPhotoDescriptionAsync(int photoId);
        
        Task<int> SavePhotoAsync(Photo photo, Stream data, bool showInFeed);

        Task RemovePhotoAsync(int photoId, int currentUserId);

        Task UpdatePhotoAsync(Photo photo, int currentUserId);

        string GetUserImageUrl(int userId);

        void CropPhoto(int photoId, int currentUserId, Rectangle cropArea);
    }
}