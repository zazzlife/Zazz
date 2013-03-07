using System.Threading.Tasks;
using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IPhotoRepository : IRepository<Photo>
    {
        Task<string> GetDescriptionAsync(int photoId);

        Task<int> GetOwnerId(int photoId);
    }
}