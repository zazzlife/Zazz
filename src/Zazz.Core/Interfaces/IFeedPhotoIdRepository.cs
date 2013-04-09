using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IFeedPhotoIdRepository : IRepository<FeedPhotoId>
    {
        void RemoveByPhotoId(int photoId);

        int GetCount(int feedId);
    }
}