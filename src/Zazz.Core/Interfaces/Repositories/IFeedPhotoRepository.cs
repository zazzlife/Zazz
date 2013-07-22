
namespace Zazz.Core.Interfaces.Repositories
{
    public interface IFeedPhotoRepository
    {
        int RemoveByPhotoIdAndReturnFeedId(int photoId);

        int GetCount(int feedId);
    }
}