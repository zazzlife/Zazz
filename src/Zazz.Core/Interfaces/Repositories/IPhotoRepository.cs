using System.Collections.Generic;
using System.Linq;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.DTOs;

namespace Zazz.Core.Interfaces.Repositories
{
    public interface IPhotoRepository : IRepository<Photo>
    {
        IQueryable<Photo> GetLatestUserPhotos(int userId, int count);

        IQueryable<Photo> GetPhotos(IEnumerable<int> photoIds);

        IQueryable<Photo> GetPagePhotos(int pageId);
            
        PhotoMinimalDTO GetPhotoWithMinimalData(int photoId);

        string GetDescription(int photoId);

        int GetOwnerId(int photoId);
        
        Photo GetByFacebookId(string fbId);

        void RemovePhotoFromUser(int userId);
    }
}