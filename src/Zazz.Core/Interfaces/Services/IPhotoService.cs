using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Zazz.Core.Models;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces.Services
{
    public interface IPhotoService
    {
        IQueryable<Photo> GetLatestUserPhotos(int userId, int count);

        IQueryable<Photo> GetAll();

        IQueryable<Photo> GetUserPhotos(int userId, int take, int? lastPhotoId = null);

        Photo GetPhoto(int id);

        PhotoLinks GeneratePhotoUrl(int userId, int photoId);

        PhotoLinks GeneratePhotoFilePath(int userId, int photoId);

        string GetPhotoDescription(int photoId);
        
        int SavePhoto(Photo photo, Stream data, bool showInFeed, IEnumerable<int> categories);

        void RemovePhoto(int photoId, int currentUserId);

        void UpdatePhoto(Photo updatedPhoto, int currentUserId);

        PhotoLinks GetUserImageUrl(int userId);

        void CropPhoto(Photo photo, int currentUserId, Rectangle cropArea);
    }
}