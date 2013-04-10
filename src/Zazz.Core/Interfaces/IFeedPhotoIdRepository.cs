using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IFeedPhotoIdRepository : IRepository<FeedPhotoId>
    {
        int RemoveByPhotoIdAndReturnFeedId(int photoId);

        int GetCount(int feedId);
    }
}