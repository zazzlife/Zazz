
namespace Zazz.Core.Interfaces
{
    public interface IFeedPhotoRepository
    {
        int RemoveByPhotoIdAndReturnFeedId(int photoId);

        int GetCount(int feedId);
    }
}