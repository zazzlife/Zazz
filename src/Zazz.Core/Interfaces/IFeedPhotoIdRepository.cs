using Zazz.Core.Models.Data;

namespace Zazz.Core.Interfaces
{
    public interface IFeedPhotoIdRepository : IRepository<FeedPhoto>
    {
        int RemoveByPhotoIdAndReturnFeedId(int photoId);

        int GetCount(int feedId);
    }
}