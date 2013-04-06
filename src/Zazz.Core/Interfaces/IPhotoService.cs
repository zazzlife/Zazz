using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IPhotoService : IDisposable
    {
        IQueryable<Photo> GetAll();

        Task<Photo> GetPhotoAsync(int id);

        string GeneratePhotoUrl(int userId, int photoId);

        string GeneratePhotoFilePath(int userId, int photoId);

        Task<string> GetPhotoDescriptionAsync(int photoId);
        
        Task<int> SavePhotoAsync(Photo photo, Stream data, bool showInFeed);

        Task RemovePhotoAsync(int photoId, int currentUserId);

        Task UpdatePhotoAsync(Photo photo, int currentUserId);

        string GetUserImageUrl(int userId);

        void CropPhoto(Photo photo, int currentUserId, Rectangle cropArea);
    }
}