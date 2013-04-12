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

        Photo GetPhoto(int id);

        string GeneratePhotoUrl(int userId, int photoId);

        string GeneratePhotoFilePath(int userId, int photoId);

        string GetPhotoDescription(int photoId);
        
        Task<int> SavePhotoAsync(Photo photo, Stream data, bool showInFeed);

        void RemovePhoto(int photoId, int currentUserId);

        void UpdatePhoto(Photo photo, int currentUserId);

        string GetUserImageUrl(int userId);

        void CropPhoto(Photo photo, int currentUserId, Rectangle cropArea);
    }
}