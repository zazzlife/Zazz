using System.Threading.Tasks;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.DTOs;

namespace Zazz.Core.Interfaces
{
    public interface IPhotoRepository : IRepository<Photo>
    {
        PhotoMinimalDTO GetPhotoWithMinimalData(int photoId);

        Task<string> GetDescriptionAsync(int photoId);

        Task<int> GetOwnerIdAsync(int photoId);
        
        Photo GetByFacebookId(string fbId);
    }
}